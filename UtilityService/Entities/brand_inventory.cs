using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UtilityService.Entities;

[Table("brand_inventory", Schema = "evm")]
public partial class brand_inventory
{
    [Key]
    public Guid vehicle_version_id { get; set; }

    public int? stock_quantity { get; set; }

    [ForeignKey("vehicle_version_id")]
    [InverseProperty("brand_inventory")]
    public virtual vehicle_version vehicle_version { get; set; } = null!;
}
