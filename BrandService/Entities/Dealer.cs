using System;
using System.Collections.Generic;

namespace BrandService.Entities;

public partial class Dealer
{
    public Guid DealerId { get; set; }

    public string DealerCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Region { get; set; }

    public string? Address { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<CustomerDebt> CustomerDebts { get; set; } = new List<CustomerDebt>();

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<DealerContract> DealerContracts { get; set; } = new List<DealerContract>();

    public virtual ICollection<DealerDiscount> DealerDiscounts { get; set; } = new List<DealerDiscount>();

    public virtual ICollection<DealerTarget> DealerTargets { get; set; } = new List<DealerTarget>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();

    public virtual ICollection<TestDrife> TestDrives { get; set; } = new List<TestDrife>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<VehicleAllocation> VehicleAllocations { get; set; } = new List<VehicleAllocation>();
}
