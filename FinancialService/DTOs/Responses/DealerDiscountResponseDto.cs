namespace FinancialService.Model;

public class DealerDiscountResponseDto
{
    public Guid dealer_discount_id { get; set; }
    public Guid dealer_id { get; set; }
    public decimal discount_rate { get; set; }
    public DateOnly valid_from { get; set; }
    public DateOnly valid_to { get; set; }
    public string status { get; set; } = null!;
    public DealerSummaryDto dealer { get; set; } = null!;
}

public class DealerSummaryDto
{
    public Guid dealer_id { get; set; }
    public string dealer_code { get; set; } = null!;
    public string name { get; set; } = null!;
    public string region { get; set; } = null!;
    public string contact_email { get; set; } = null!;
    public string contact_phone { get; set; } = null!;
    public string status { get; set; } = null!;
}