using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("orders", Schema = "evm")]
public partial class order
{
    [Key]
    public Guid order_id { get; set; }

    public Guid dealer_id { get; set; }

    public Guid contract_id { get; set; }

    public Guid customer_id { get; set; }

    [StringLength(300)]
    public string? delivery_address { get; set; }

    public DateOnly? delivery_date { get; set; }

    [StringLength(30)]
    public string status { get; set; } = null!;

    [ForeignKey("contract_id")]
    [InverseProperty("orders")]
    public virtual contract contract { get; set; } = null!;

    [ForeignKey("customer_id")]
    [InverseProperty("orders")]
    public virtual customer customer { get; set; } = null!;

    [ForeignKey("dealer_id")]
    [InverseProperty("orders")]
    public virtual dealer dealer { get; set; } = null!;
}
