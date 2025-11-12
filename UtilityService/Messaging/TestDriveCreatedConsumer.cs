using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using UtilityService.Infrastructure.Services;
using UtilityService.Models;

namespace UtilityService.Messaging;

public class TestDriveCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<TestDriveCreatedConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TestDriveCreatedConsumer(
        IConfiguration cfg,
        ILogger<TestDriveCreatedConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _cfg = cfg;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerCfg = new ConsumerConfig
        {
            BootstrapServers = _cfg["KafkaSettings:BootstrapServers"],
            GroupId = "utility-service-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerCfg).Build();
        var topic = _cfg["KafkaSettings:Topics:Dealer"];
        consumer.Subscribe(topic);

        _logger.LogInformation("Kafka consumer started. Topic={Topic}, GroupId={Group}",
            topic, "utility-service-consumer");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var cr = consumer.Consume(stoppingToken); 
                _logger.LogInformation(
                    "Received message from topic {Topic}, partition {Partition}, offset {Offset}. Raw value = {Value}",
                    cr.Topic, cr.Partition, cr.Offset, cr.Message.Value);

                try
                {
                    var ev = JsonSerializer.Deserialize<TestDriveCreatedEvent>(cr.Message.Value, _jsonOpts);

                    if (ev == null)
                    {
                        _logger.LogWarning("Deserialized event is null. Raw={Raw}", cr.Message.Value);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Parsed TestDriveCreatedEvent: Id={TestDriveId}, Dealer={DealerId}, ConfirmEmail={ConfirmEmail}, Email={Email}",
                            ev.TestDriveId, ev.DealerId, ev.ConfirmEmail, ev.Email);

                        if (ev.ConfirmEmail && !string.IsNullOrWhiteSpace(ev.Email))
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                            var emailDto = new EmailRequestDto
                            {
                                ToEmail = ev.Email,
                                Subject = $"Xác nhận lịch lái thử #{ev.TestDriveId}",
                                Content = $"Bạn đã đặt lịch lái thử ngày {ev.DriveDate:dd/MM/yyyy} - {ev.TimeSlot}"
                            };

                            _logger.LogInformation("Sending email to {To} ...", emailDto.ToEmail);
                            var result = await emailService.SendEmailAsync(emailDto);
                            _logger.LogInformation("Sent email result: status={Status}, message={Message}", result.Status, result.Message);
                        }
                    }

                    consumer.Commit(cr);
                    _logger.LogInformation("Committed offset {Offset} for partition {Partition}", cr.Offset, cr.Partition);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Process failed at {Topic}-{PartitionOffset}", cr.Topic, cr.TopicPartitionOffset);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumer stopping...");
        }
        finally
        {
            consumer.Close();
            _logger.LogInformation("Kafka consumer closed");
        }
    }
}

public record TestDriveCreatedEvent(
    Guid TestDriveId,
    Guid DealerId,
    Guid CustomerId,
    Guid VehicleVersionId,
    DateTime DriveDate,
    string TimeSlot,
    bool ConfirmSms,
    string Email,
    bool ConfirmEmail,
    string Status,
    DateTime OccurredAtUtc
);
