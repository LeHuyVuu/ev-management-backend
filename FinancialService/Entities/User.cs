using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("users", Schema = "evm")]
[Index("dealer_id", Name = "idx_users_dealer")]
[Index("email", Name = "users_email_key", IsUnique = true)]
public partial class user
{
    [Key]
    public Guid user_id { get; set; }

    [StringLength(200)]
    public string name { get; set; } = null!;

    [StringLength(200)]
    public string email { get; set; } = null!;

    [StringLength(50)]
    public string? phone { get; set; }

    [StringLength(200)]
    public string password_hash { get; set; } = null!;

    public int role_id { get; set; }

    public Guid? dealer_id { get; set; }

    [StringLength(30)]
    public string status { get; set; } = null!;

    public DateOnly? last_activity_at { get; set; }

    [ForeignKey("dealer_id")]
    [InverseProperty("users")]
    public virtual dealer? dealer { get; set; }

    [InverseProperty("created_by_userNavigation")]
    public virtual ICollection<quote> quotes { get; set; } = new List<quote>();

    [ForeignKey("role_id")]
    [InverseProperty("users")]
    public virtual role role { get; set; } = null!;

    [InverseProperty("staff_user")]
    public virtual ICollection<test_drife> test_drives { get; set; } = new List<test_drife>();
}
