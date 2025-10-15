namespace OrderService.Entities;

public partial class CustomerDebt
{
    public Guid CustomerDebtId { get; set; }

    public Guid DealerId { get; set; }

    public Guid CustomerId { get; set; }

    public decimal Amount { get; set; }

    public DateOnly DueDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Dealer Dealer { get; set; } = null!;
}
