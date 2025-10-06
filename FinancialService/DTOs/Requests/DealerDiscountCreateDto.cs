namespace FinancialService.Model;

public class DealerDiscountCreateDto
{
    public Guid dealer_id { get; set; }
    public decimal discount_rate { get; set; }
    public string valid_from { get; set; } = null!;
    public string valid_to { get; set; } = null!;
}