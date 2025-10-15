using System;
using System.Collections.Generic;

namespace OrderService.Entities;

public partial class VehicleVersion
{
    public Guid VehicleVersionId { get; set; }

    public Guid VehicleId { get; set; }

    public string VersionName { get; set; } = null!;

    public string? Color { get; set; }

    public string? EvType { get; set; }

    public int? HorsePower { get; set; }

    public decimal BasePrice { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();

    public virtual ICollection<TestDrife> TestDrives { get; set; } = new List<TestDrife>();

    public virtual Vehicle Vehicle { get; set; } = null!;

    public virtual ICollection<VehicleAllocation> VehicleAllocations { get; set; } = new List<VehicleAllocation>();

    public virtual ICollection<WholesalePrice> WholesalePrices { get; set; } = new List<WholesalePrice>();
}
