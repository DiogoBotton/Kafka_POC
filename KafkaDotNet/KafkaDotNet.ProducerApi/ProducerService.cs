using Confluent.Kafka;
using Newtonsoft.Json;

namespace KafkaDotNet.ProducerApi;

public class ProducerService
{
    private readonly ILogger<ProducerService> _logger;
    private const string TOPIC_NAME = "kafka-topic-test";

    public ProducerService(ILogger<ProducerService> logger)
    {
        _logger = logger;
    }

    public class DataStreamingController
    {
        public DataStreamingController()
        {
            topic = TOPIC_NAME;
            Random random = new Random();
            value = random.Next(15, 50);
        }

        public string topic { get; set; }
        public int value { get; set; }
    }

    public async Task ProduceAsync(CancellationToken cancellationToken)
    {
        var config = new ProducerConfig
        {
            //BootstrapServers = "localhost:9092",
            BootstrapServers = "kafka:29092",
            AllowAutoCreateTopics = true,
            Acks = Acks.All
        };

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        try
        {
            while (true)
            {
                DataStreamingController controller = new DataStreamingController();
                Thread.Sleep(5000);
                var deliveryResult = await producer.ProduceAsync(TOPIC_NAME,
                new Message<Null, string>
                {
                    Value = JsonConvert.SerializeObject(controller)
                },
                cancellationToken);

                _logger.LogInformation($"Delivered message to {deliveryResult.Value}, Offset: {deliveryResult.Offset}");
            }
        }
        catch (ProduceException<Null, string> e)
        {
            _logger.LogError($"Delivery failed: {e.Error.Reason}");
        }

        producer.Flush(cancellationToken);
    }
}
