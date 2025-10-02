using System;
using System.Collections.Generic;

namespace ProductService.Entities;

public partial class User
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public Guid? DealerId { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly? LastActivityAt { get; set; }

    public virtual Dealer? Dealer { get; set; }

    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<TestDrife> TestDrives { get; set; } = new List<TestDrife>();
}
