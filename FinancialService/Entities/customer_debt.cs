using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("customer_debts", Schema = "evm")]
public partial class customer_debt
{
    [Key]
    public Guid customer_debt_id { get; set; }

    public Guid dealer_id { get; set; }

    public Guid customer_id { get; set; }

    [Precision(18, 2)]
    public decimal amount { get; set; }

    public DateOnly due_date { get; set; }

    [StringLength(30)]
    public string status { get; set; } = null!;

    [ForeignKey("customer_id")]
    [InverseProperty("customer_debts")]
    public virtual customer customer { get; set; } = null!;

    [ForeignKey("dealer_id")]
    [InverseProperty("customer_debts")]
    public virtual dealer dealer { get; set; } = null!;
}
