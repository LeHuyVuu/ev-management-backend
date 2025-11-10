namespace UtilityService.Models;

public class ForecastRow
{
    public Guid dealer_id { get; set; }
    public string dealer_name { get; set; } = "";
    public string? region { get; set; }
    public Guid vehicle_version_id { get; set; }
    public string version_name { get; set; } = "";
    public DateTime week_start { get; set; }
    public decimal baseline_ema { get; set; }
    public decimal discount_rate_active { get; set; }
    public int has_promo { get; set; }
    public decimal forecast_units { get; set; }
}