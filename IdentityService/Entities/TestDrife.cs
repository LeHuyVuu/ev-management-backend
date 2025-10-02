namespace IdentityService.Entities;

public partial class TestDrife
{
    public Guid TestDriveId { get; set; }

    public Guid DealerId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid VehicleVersionId { get; set; }

    public Guid? StaffUserId { get; set; }

    public DateOnly DriveDate { get; set; }

    public string TimeSlot { get; set; } = null!;

    public bool ConfirmSms { get; set; }

    public bool ConfirmEmail { get; set; }

    public string Status { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Dealer Dealer { get; set; } = null!;

    public virtual User? StaffUser { get; set; }

    public virtual VehicleVersion VehicleVersion { get; set; } = null!;
}
