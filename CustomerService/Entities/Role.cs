﻿namespace CustomerService.Entities;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleKey { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
