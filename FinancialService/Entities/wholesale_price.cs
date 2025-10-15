using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("wholesale_prices", Schema = "evm")]
public partial class wholesale_price
{
    [Key]
    public Guid wholesale_price_id { get; set; }

    public Guid vehicle_version_id { get; set; }

    [Column("wholesale_price")]
    [Precision(18, 2)]
    public decimal wholesale_price1 { get; set; }

    public int min_order_quantity { get; set; }

    public DateOnly valid_from { get; set; }

    public DateOnly? valid_to { get; set; }

    [ForeignKey("vehicle_version_id")]
    [InverseProperty("wholesale_prices")]
    public virtual vehicle_version vehicle_version { get; set; } = null!;
}
