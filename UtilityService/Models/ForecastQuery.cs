namespace UtilityService.Models;

public class ForecastQuery
{
    public Guid? DealerId { get; set; }
    public Guid? VehicleVersionId { get; set; }
    public DateTime? FromWeek { get; set; }
    public DateTime? ToWeek { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}