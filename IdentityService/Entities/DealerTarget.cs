namespace IdentityService.Entities;

public partial class DealerTarget
{
    public Guid DealerTargetId { get; set; }

    public Guid DealerId { get; set; }

    public string Period { get; set; } = null!;

    public decimal TargetAmount { get; set; }

    public decimal AchievedAmount { get; set; }

    public DateOnly StartDate { get; set; }

    public virtual Dealer Dealer { get; set; } = null!;
}
