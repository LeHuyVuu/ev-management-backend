using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("promotions", Schema = "evm")]
public partial class promotion
{
    [Key]
    public Guid promotion_id { get; set; }

    [StringLength(200)]
    public string name { get; set; } = null!;

    [StringLength(50)]
    public string type { get; set; } = null!;

    public DateOnly start_date { get; set; }

    public DateOnly end_date { get; set; }

    [StringLength(30)]
    public string status { get; set; } = null!;

    public string? description { get; set; }
}
