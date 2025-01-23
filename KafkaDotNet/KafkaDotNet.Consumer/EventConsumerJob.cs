using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaDotNet.Consumer;

internal class EventConsumerJob : BackgroundService
{
    private readonly ILogger<EventConsumerJob> _logger;
    private const string TOPIC_NAME = "kafka-topic-test";

    public EventConsumerJob(ILogger<EventConsumerJob> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            //BootstrapServers = "localhost:9092",
            BootstrapServers = "kafka:29092",
            GroupId = "test-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        consumer.Subscribe(TOPIC_NAME);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(TimeSpan.FromSeconds(5));

                if (consumeResult == null)
                {
                    continue;
                }

                _logger.LogInformation($"Consumed message '{consumeResult.Message.Value}' at: '{consumeResult.Offset}'");
            }
            catch (Exception)
            {
                // Ignore
            }
        }


        return Task.CompletedTask;
    }
}
