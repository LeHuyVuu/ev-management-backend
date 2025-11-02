namespace OrderService.Kafka.Events;

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
