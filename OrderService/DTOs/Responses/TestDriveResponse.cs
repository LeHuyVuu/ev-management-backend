namespace OrderService.DTOs.Responses;

public class TestDriveResponse
{
    public Guid TestDriveId { get; set; }

    public string DealerName { get; set; }

    public string CustomerName { get; set; }

    public string VehicleName { get; set; }

    public string StaffUserName { get; set; }

    public DateOnly DriveDate { get; set; }

    public string TimeSlot { get; set; } = null!;

    public bool ConfirmSms { get; set; }

    public bool ConfirmEmail { get; set; }

    public string Status { get; set; } = null!;
}