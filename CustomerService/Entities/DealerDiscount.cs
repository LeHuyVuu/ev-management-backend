using System;
using System.Collections.Generic;

namespace ProductService.Entities;

public partial class DealerDiscount
{
    public Guid DealerDiscountId { get; set; }

    public Guid DealerId { get; set; }

    public decimal DiscountRate { get; set; }

    public DateOnly ValidFrom { get; set; }

    public DateOnly ValidTo { get; set; }

    public string Status { get; set; } = null!;

    public virtual Dealer Dealer { get; set; } = null!;
}
