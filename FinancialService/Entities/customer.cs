using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("customers", Schema = "evm")]
[Index("dealer_id", Name = "idx_customers_dealer")]
public partial class customer
{
    [Key]
    public Guid customer_id { get; set; }

    public Guid dealer_id { get; set; }

    [StringLength(200)]
    public string name { get; set; } = null!;

    [StringLength(200)]
    public string? email { get; set; }

    [StringLength(50)]
    public string? phone { get; set; }

    [StringLength(300)]
    public string? address { get; set; }

    [StringLength(30)]
    public string status { get; set; } = null!;

    public DateOnly? last_interaction_date { get; set; }

    [InverseProperty("customer")]
    public virtual ICollection<contract> contracts { get; set; } = new List<contract>();

    [InverseProperty("customer")]
    public virtual ICollection<customer_debt> customer_debts { get; set; } = new List<customer_debt>();

    [ForeignKey("dealer_id")]
    [InverseProperty("customers")]
    public virtual dealer dealer { get; set; } = null!;

    [InverseProperty("customer")]
    public virtual ICollection<order> orders { get; set; } = new List<order>();

    [InverseProperty("customer")]
    public virtual ICollection<quote> quotes { get; set; } = new List<quote>();

    [InverseProperty("customer")]
    public virtual ICollection<test_drife> test_drives { get; set; } = new List<test_drife>();
}
