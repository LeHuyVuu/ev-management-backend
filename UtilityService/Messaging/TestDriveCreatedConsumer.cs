using System.Text.Json;
using Confluent.Kafka;

using UtilityService.Models;
using UtilityService.Infrastructure.Services;

namespace UtilityService.Messaging;

public class TestDriveCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<TestDriveCreatedConsumer> _logger;
    private readonly EmailService _emailService;

    private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions {
        PropertyNameCaseInsensitive = true   // 👈 fix mapping camelCase -> PascalCase
    };

    public TestDriveCreatedConsumer(
        IConfiguration cfg,
        ILogger<TestDriveCreatedConsumer> logger,
        EmailService emailService)
    {
        _cfg = cfg;
        _logger = logger;
        _emailService = emailService;
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
        var topic = _cfg["KafkaSettings:Topic"];
        consumer.Subscribe(topic);

        _logger.LogInformation("Kafka consumer started. Topic={Topic}, GroupId={Group}",
            topic, "utility-service-consumer");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var cr = consumer.Consume(stoppingToken);
                _logger.LogInformation("Received message from topic {Topic}, partition {Partition}, offset {Offset}. Raw value = {Value}",
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
                        if (ev.TestDriveId == Guid.Empty || ev.DealerId == Guid.Empty)
                            _logger.LogWarning("Event has empty IDs after parse. Parsed={@Ev}", ev);

                        _logger.LogInformation(
                            "Parsed TestDriveCreatedEvent: Id={TestDriveId}, Dealer={DealerId}, ConfirmEmail={ConfirmEmail}",
                            ev.TestDriveId, ev.DealerId, ev.ConfirmEmail);

                        if (ev.ConfirmEmail)
                        {
                            var emailDto = new EmailRequestDto()
                            {
                                ToEmail = "trungthe200427@gmail.com", // TODO lookup thật
                                Subject = $"Xác nhận lịch lái thử #{ev.TestDriveId}",
                                Content = $"Bạn đã đặt lịch lái thử ngày {ev.DriveDate:dd/MM/yyyy} - {ev.TimeSlot}"
                            };

                            _logger.LogInformation("Sending email to {To} ...", emailDto.ToEmail);
                            var result = await _emailService.SendEmailAsync(emailDto);
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
    bool ConfirmEmail,
    string Status,
    DateTime OccurredAtUtc
);
