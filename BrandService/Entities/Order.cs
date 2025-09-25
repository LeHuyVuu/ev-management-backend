using System;
using System.Collections.Generic;

namespace BrandService.Entities;

public partial class Order
{
    public Guid OrderId { get; set; }

    public Guid DealerId { get; set; }

    public Guid ContractId { get; set; }

    public Guid CustomerId { get; set; }

    public string? DeliveryAddress { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Contract Contract { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Dealer Dealer { get; set; } = null!;
}
