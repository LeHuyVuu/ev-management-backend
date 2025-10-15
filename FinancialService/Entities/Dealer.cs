using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("dealers", Schema = "evm")]
[Index("dealer_code", Name = "dealers_dealer_code_key", IsUnique = true)]
public partial class dealer
{
    [Key]
    public Guid dealer_id { get; set; }

    [StringLength(50)]
    public string dealer_code { get; set; } = null!;

    [StringLength(200)]
    public string name { get; set; } = null!;

    [StringLength(100)]
    public string? region { get; set; }

    [StringLength(300)]
    public string? address { get; set; }

    [StringLength(200)]
    public string? contact_email { get; set; }

    [StringLength(50)]
    public string? contact_phone { get; set; }

    [StringLength(30)]
    public string status { get; set; } = null!;

    [InverseProperty("dealer")]
    public virtual ICollection<contract> contracts { get; set; } = new List<contract>();

    [InverseProperty("dealer")]
    public virtual ICollection<customer_debt> customer_debts { get; set; } = new List<customer_debt>();

    [InverseProperty("dealer")]
    public virtual ICollection<customer> customers { get; set; } = new List<customer>();

    [InverseProperty("dealer")]
    public virtual ICollection<dealer_contract> dealer_contracts { get; set; } = new List<dealer_contract>();

    [InverseProperty("dealer")]
    public virtual ICollection<dealer_discount> dealer_discounts { get; set; } = new List<dealer_discount>();

    [InverseProperty("dealer")]
    public virtual ICollection<dealer_target> dealer_targets { get; set; } = new List<dealer_target>();

    [InverseProperty("dealer")]
    public virtual ICollection<inventory> inventories { get; set; } = new List<inventory>();

    [InverseProperty("dealer")]
    public virtual ICollection<order> orders { get; set; } = new List<order>();

    [InverseProperty("dealer")]
    public virtual ICollection<quote> quotes { get; set; } = new List<quote>();

    [InverseProperty("dealer")]
    public virtual ICollection<test_drife> test_drives { get; set; } = new List<test_drife>();

    [InverseProperty("dealer")]
    public virtual ICollection<user> users { get; set; } = new List<user>();

    [InverseProperty("dealer")]
    public virtual ICollection<vehicle_allocation> vehicle_allocations { get; set; } = new List<vehicle_allocation>();
}
