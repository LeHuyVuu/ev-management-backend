namespace OrderService.Entities;

public partial class DealerContract
{
    public Guid DealerContractId { get; set; }

    public Guid DealerId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Dealer Dealer { get; set; } = null!;
}
