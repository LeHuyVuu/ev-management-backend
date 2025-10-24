using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UtilityService.Entities;

[Table("vehicle_transfer_order", Schema = "evm")]
public partial class vehicle_transfer_order
{
    [Key]
    public Guid vehicle_transfer_order_id { get; set; }

    public Guid? from_dealer_id { get; set; }

    public Guid? to_dealer_id { get; set; }

    public Guid? vehicle_version_id { get; set; }

    public int? quantity { get; set; }

    public DateOnly? request_date { get; set; }

    [StringLength(30)]
    public string? status { get; set; }

    [ForeignKey("from_dealer_id")]
    [InverseProperty("vehicle_transfer_orderfrom_dealers")]
    public virtual dealer? from_dealer { get; set; }

    [ForeignKey("to_dealer_id")]
    [InverseProperty("vehicle_transfer_orderto_dealers")]
    public virtual dealer? to_dealer { get; set; }

    [ForeignKey("vehicle_version_id")]
    [InverseProperty("vehicle_transfer_orders")]
    public virtual vehicle_version? vehicle_version { get; set; }
}
