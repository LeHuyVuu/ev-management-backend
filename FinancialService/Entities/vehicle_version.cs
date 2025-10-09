using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("vehicle_versions", Schema = "evm")]
[Index("vehicle_id", "version_name", "color", Name = "vehicle_versions_vehicle_id_version_name_color_key", IsUnique = true)]
public partial class vehicle_version
{
    [Key]
    public Guid vehicle_version_id { get; set; }

    public Guid vehicle_id { get; set; }

    [StringLength(150)]
    public string version_name { get; set; } = null!;

    [StringLength(100)]
    public string? color { get; set; }

    [StringLength(50)]
    public string? ev_type { get; set; }

    public int? horse_power { get; set; }

    [Precision(18, 2)]
    public decimal base_price { get; set; }

    [InverseProperty("vehicle_version")]
    public virtual ICollection<inventory> inventories { get; set; } = new List<inventory>();

    [InverseProperty("vehicle_version")]
    public virtual ICollection<quote> quotes { get; set; } = new List<quote>();

    [InverseProperty("vehicle_version")]
    public virtual ICollection<test_drife> test_drives { get; set; } = new List<test_drife>();

    [ForeignKey("vehicle_id")]
    [InverseProperty("vehicle_versions")]
    public virtual vehicle vehicle { get; set; } = null!;

    [InverseProperty("vehicle_version")]
    public virtual ICollection<vehicle_allocation> vehicle_allocations { get; set; } = new List<vehicle_allocation>();

    [InverseProperty("vehicle_version")]
    public virtual ICollection<wholesale_price> wholesale_prices { get; set; } = new List<wholesale_price>();
}
