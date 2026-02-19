using BillingMicroservice.Models;
using Confluent.Kafka;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace BillingMicroservice.BGServices;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaConsumerService> _logger;
    public readonly IMongoCollection<BillModel> _bills;

    public KafkaConsumerService(IConfiguration configuration, ILogger<KafkaConsumerService> logger, BillingDbContext context)
    {
        _configuration = configuration;
        _logger = logger;
        _bills = context.Bills;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_configuration["Kafka:Topic"]);

        _logger.LogInformation("Kafka consumer started for topic: {Topic}", _configuration["Kafka:Topic"]);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);

                if(consumeResult != null)
                {
                    var eventPayload = JsonConvert.DeserializeObject<BillRequest>(consumeResult.Message.Value);

                    _logger.LogInformation("Received event for DeviceId: {DeviceId}", eventPayload.DeviceId);

                    var discountAmount = (eventPayload.Price * eventPayload.Discount) / 100;
                    var priceAfterDiscount = eventPayload.Price - discountAmount;
                    var taxAmount = (priceAfterDiscount * eventPayload.Tax) / 100;
                    var finalPrice = priceAfterDiscount + taxAmount;

                    var bill = new BillModel
                    {
                        DeviceId = eventPayload.DeviceId,
                        DeviceName = eventPayload.DeviceName,
                        Price = eventPayload.Price,
                        Discount = eventPayload.Discount,
                        Tax = eventPayload.Tax,
                        ManufactureDate = eventPayload.ManufactureDate,
                        FinalPrice = finalPrice,
                        SellingDate = DateTime.UtcNow
                    };

                    await _bills.InsertOneAsync(bill);
                }
            }
            catch (ConsumeException ex)
            {
                _logger.LogError("Consume error: {Message}", ex.Message);
            }
            catch (OperationCanceledException) { }
        }
        
        consumer.Close();
    }
}