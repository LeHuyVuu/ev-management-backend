using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("manufacturer_debts", Schema = "evm")]
public partial class manufacturer_debt
{
    [Key]
    public Guid manufacturer_debt_id { get; set; }

    [StringLength(200)]
    public string manufacturer_name { get; set; } = null!;

    [Precision(18, 2)]
    public decimal amount { get; set; }

    public DateOnly due_date { get; set; }

    [StringLength(30)]
    public string status { get; set; } = null!;
}
