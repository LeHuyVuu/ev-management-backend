namespace CustomerService.DTOs.Requests.QuoteDTOs;

public class QuoteCreateRequest
{
    public string CustomerName { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;

    public string Brand { get; set; } = null!;
    public string ModelName { get; set; } = null!;

    public string VersionName { get; set; } = null!;
    public string Color { get; set; } = null!;

    public List<Guid>? PromotionIds { get; set; }

    public decimal? DiscountAmt { get; set; }

    // default to draft if not provided
    public string? Status { get; set; }
}
