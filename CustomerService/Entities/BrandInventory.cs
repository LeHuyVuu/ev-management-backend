using System;
using System.Collections.Generic;

namespace CustomerService.Entities;

public partial class BrandInventory
{
    public Guid VehicleVersionId { get; set; }

    public int? StockQuantity { get; set; }

    public virtual VehicleVersion VehicleVersion { get; set; } = null!;
}
