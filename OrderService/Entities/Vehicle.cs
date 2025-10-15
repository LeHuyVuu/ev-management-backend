using System;
using System.Collections.Generic;

namespace OrderService.Entities;

public partial class Vehicle
{
    public Guid VehicleId { get; set; }

    public string Brand { get; set; } = null!;

    public string ModelName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<VehicleVersion> VehicleVersions { get; set; } = new List<VehicleVersion>();
}
