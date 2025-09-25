using System;
using System.Collections.Generic;

namespace BrandService.Entities;

public partial class DealerContract
{
    public Guid DealerContractId { get; set; }

    public Guid DealerId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Dealer Dealer { get; set; } = null!;
}
