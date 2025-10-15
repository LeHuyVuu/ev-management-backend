using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("quotes", Schema = "evm")]
[Index("dealer_id", Name = "idx_quotes_dealer")]
public partial class quote
{
    [Key]
    public Guid quote_id { get; set; }

    public Guid dealer_id { get; set; }

    public Guid customer_id { get; set; }

    public Guid vehicle_version_id { get; set; }

    [Column(TypeName = "jsonb")]
    public string? options_json { get; set; }

    [StringLength(100)]
    public string? discount_code { get; set; }

    [Precision(18, 2)]
    public decimal subtotal { get; set; }

    [Precision(18, 2)]
    public decimal discount_amt { get; set; }

    [Precision(18, 2)]
    public decimal total_price { get; set; }

    [StringLength(30)]
    public string status { get; set; } = null!;

    public Guid created_by_user { get; set; }

    [InverseProperty("quote")]
    public virtual ICollection<contract> contracts { get; set; } = new List<contract>();

    [ForeignKey("created_by_user")]
    [InverseProperty("quotes")]
    public virtual user created_by_userNavigation { get; set; } = null!;

    [ForeignKey("customer_id")]
    [InverseProperty("quotes")]
    public virtual customer customer { get; set; } = null!;

    [ForeignKey("dealer_id")]
    [InverseProperty("quotes")]
    public virtual dealer dealer { get; set; } = null!;

    [ForeignKey("vehicle_version_id")]
    [InverseProperty("quotes")]
    public virtual vehicle_version vehicle_version { get; set; } = null!;
}
