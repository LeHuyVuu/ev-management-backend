namespace DealerService.Entities;

public partial class Contract
{
    public Guid ContractId { get; set; }

    public Guid DealerId { get; set; }

    public Guid? QuoteId { get; set; }

    public Guid CustomerId { get; set; }

    public decimal TotalValue { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateOnly? SignedDate { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Dealer Dealer { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Quote? Quote { get; set; }
}
