namespace UtilityService.Models;

public class AskRequestDto
{
    public string Question { get; set; } = "";
    // Optional: FE có thể đính kèm slice dữ liệu hiện tại
    public List<ForecastRow>? Forecast { get; set; }
}