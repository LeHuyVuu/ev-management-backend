using System;
using System.Collections.Generic;

namespace DealerService.Entities;

public partial class Promotion
{
    public Guid PromotionId { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Description { get; set; }
}
