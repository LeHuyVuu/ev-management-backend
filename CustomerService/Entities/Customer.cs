using System;
using System.Collections.Generic;

namespace ProductService.Entities;

public partial class Customer
{
    public Guid CustomerId { get; set; }

    public Guid DealerId { get; set; }

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly? LastInteractionDate { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<CustomerDebt> CustomerDebts { get; set; } = new List<CustomerDebt>();

    public virtual Dealer Dealer { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();

    public virtual ICollection<TestDrife> TestDrives { get; set; } = new List<TestDrife>();
}
