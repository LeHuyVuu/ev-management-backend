using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("permissions", Schema = "evm")]
[Index("permission_key", Name = "permissions_permission_key_key", IsUnique = true)]
public partial class permission
{
    [Key]
    public int permission_id { get; set; }

    [StringLength(100)]
    public string permission_key { get; set; } = null!;

    [StringLength(200)]
    public string permission_name { get; set; } = null!;

    [ForeignKey("permission_id")]
    [InverseProperty("permissions")]
    public virtual ICollection<role> roles { get; set; } = new List<role>();
}
