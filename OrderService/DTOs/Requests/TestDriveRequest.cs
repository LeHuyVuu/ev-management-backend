namespace OrderService.DTOs.Requests;

public class TestDriveRequest
{
    public Guid CustomerId { get; set; }

    public Guid VehicleVersionId { get; set; }
    
    public Guid? StaffUserId { get; set; }

    public DateTime DriveDate { get; set; }

    public string TimeSlot { get; set; } = null!;

    public bool ConfirmSms { get; set; }

    public bool ConfirmEmail { get; set; }

    public string Status { get; set; } = null!;
}