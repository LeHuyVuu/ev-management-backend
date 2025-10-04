using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("dealer_discounts", Schema = "evm")]
public partial class dealer_discount
{
    [Key]
    public Guid dealer_discount_id { get; set; }

    public Guid dealer_id { get; set; }

    [Precision(5, 2)]
    public decimal discount_rate { get; set; }

    public DateOnly valid_from { get; set; }

    public DateOnly valid_to { get; set; }

    [StringLength(30)]
    public string status { get; set; } = null!;

    [ForeignKey("dealer_id")]
    [InverseProperty("dealer_discounts")]
    public virtual dealer dealer { get; set; } = null!;
}
