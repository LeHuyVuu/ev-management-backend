using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("inventory", Schema = "evm")]
[Index("dealer_id", "vehicle_version_id", Name = "inventory_dealer_id_vehicle_version_id_key", IsUnique = true)]
public partial class inventory
{
    [Key]
    public Guid inventory_id { get; set; }

    public Guid dealer_id { get; set; }

    public Guid vehicle_version_id { get; set; }

    public int stock_quantity { get; set; }

    public DateOnly last_updated { get; set; }

    [ForeignKey("dealer_id")]
    [InverseProperty("inventories")]
    public virtual dealer dealer { get; set; } = null!;

    [ForeignKey("vehicle_version_id")]
    [InverseProperty("inventories")]
    public virtual vehicle_version vehicle_version { get; set; } = null!;
}
