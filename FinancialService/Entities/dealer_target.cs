using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("dealer_targets", Schema = "evm")]
public partial class dealer_target
{
    [Key]
    public Guid dealer_target_id { get; set; }

    public Guid dealer_id { get; set; }

    [StringLength(30)]
    public string period { get; set; } = null!;

    [Precision(18, 2)]
    public decimal target_amount { get; set; }

    [Precision(18, 2)]
    public decimal achieved_amount { get; set; }

    public DateOnly start_date { get; set; }

    [ForeignKey("dealer_id")]
    [InverseProperty("dealer_targets")]
    public virtual dealer dealer { get; set; } = null!;
}
