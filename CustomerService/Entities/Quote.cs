namespace CustomerService.Entities;

public partial class Quote
{
    public Guid QuoteId { get; set; }

    public Guid DealerId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid VehicleVersionId { get; set; }

    public string? OptionsJson { get; set; }

    public string? DiscountCode { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmt { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = null!;

    public Guid CreatedByUser { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual User CreatedByUserNavigation { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Dealer Dealer { get; set; } = null!;

    public virtual VehicleVersion VehicleVersion { get; set; } = null!;
}
