using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("roles", Schema = "evm")]
[Index("role_key", Name = "roles_role_key_key", IsUnique = true)]
public partial class role
{
    [Key]
    public int role_id { get; set; }

    [StringLength(50)]
    public string role_key { get; set; } = null!;

    [StringLength(100)]
    public string role_name { get; set; } = null!;

    [InverseProperty("role")]
    public virtual ICollection<user> users { get; set; } = new List<user>();

    [ForeignKey("role_id")]
    [InverseProperty("roles")]
    public virtual ICollection<permission> permissions { get; set; } = new List<permission>();
}
