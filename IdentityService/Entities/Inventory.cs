using System;
using System.Collections.Generic;

namespace IdentityService.Entities;

public partial class Inventory
{
    public Guid InventoryId { get; set; }

    public Guid DealerId { get; set; }

    public Guid VehicleVersionId { get; set; }

    public int StockQuantity { get; set; }

    public DateOnly LastUpdated { get; set; }

    public virtual Dealer Dealer { get; set; } = null!;

    public virtual VehicleVersion VehicleVersion { get; set; } = null!;
}
