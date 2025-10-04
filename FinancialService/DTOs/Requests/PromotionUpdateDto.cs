namespace FinancialService.Model;

public class PromotionUpdateDto
{
    public string name { get; set; } = null!;
    public string type { get; set; } = null!;
    public string start_date { get; set; } = null!;
    public string end_date { get; set; } = null!;
    public string? description { get; set; }
}