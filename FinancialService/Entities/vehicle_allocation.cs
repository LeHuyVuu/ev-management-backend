using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("vehicle_allocations", Schema = "evm")]
public partial class vehicle_allocation
{
    [Key]
    public Guid allocation_id { get; set; }

    public Guid dealer_id { get; set; }

    public Guid vehicle_version_id { get; set; }

    public int quantity { get; set; }

    public DateOnly request_date { get; set; }

    public DateOnly? expected_delivery { get; set; }

    [StringLength(30)]
    public string status { get; set; } = null!;

    [ForeignKey("dealer_id")]
    [InverseProperty("vehicle_allocations")]
    public virtual dealer dealer { get; set; } = null!;

    [ForeignKey("vehicle_version_id")]
    [InverseProperty("vehicle_allocations")]
    public virtual vehicle_version vehicle_version { get; set; } = null!;
}
