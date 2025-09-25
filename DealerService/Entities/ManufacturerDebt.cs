using System;
using System.Collections.Generic;

namespace ProductService.Entities;

public partial class ManufacturerDebt
{
    public Guid ManufacturerDebtId { get; set; }

    public string ManufacturerName { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateOnly DueDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
