using System;
using System.Collections.Generic;

namespace ProductService.Entities;

public partial class WholesalePrice
{
    public Guid WholesalePriceId { get; set; }

    public Guid VehicleVersionId { get; set; }

    public decimal WholesalePrice1 { get; set; }

    public int MinOrderQuantity { get; set; }

    public DateOnly ValidFrom { get; set; }

    public DateOnly? ValidTo { get; set; }

    public virtual VehicleVersion VehicleVersion { get; set; } = null!;
}
