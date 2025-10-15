using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("vehicles", Schema = "evm")]
[Index("brand", "model_name", Name = "vehicles_brand_model_name_key", IsUnique = true)]
public partial class vehicle
{
    [Key]
    public Guid vehicle_id { get; set; }

    [StringLength(100)]
    public string brand { get; set; } = null!;

    [StringLength(150)]
    public string model_name { get; set; } = null!;

    public string? description { get; set; }

    [InverseProperty("vehicle")]
    public virtual ICollection<vehicle_version> vehicle_versions { get; set; } = new List<vehicle_version>();
}
