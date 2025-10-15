using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("contracts", Schema = "evm")]
public partial class contract
{
    [Key]
    public Guid contract_id { get; set; }

    public Guid dealer_id { get; set; }

    public Guid? quote_id { get; set; }

    public Guid customer_id { get; set; }

    [Precision(18, 2)]
    public decimal total_value { get; set; }

    [StringLength(30)]
    public string payment_method { get; set; } = null!;

    [StringLength(30)]
    public string status { get; set; } = null!;

    public DateOnly? signed_date { get; set; }

    [ForeignKey("customer_id")]
    [InverseProperty("contracts")]
    public virtual customer customer { get; set; } = null!;

    [ForeignKey("dealer_id")]
    [InverseProperty("contracts")]
    public virtual dealer dealer { get; set; } = null!;

    [InverseProperty("contract")]
    public virtual ICollection<order> orders { get; set; } = new List<order>();

    [ForeignKey("quote_id")]
    [InverseProperty("contracts")]
    public virtual quote? quote { get; set; }
}
