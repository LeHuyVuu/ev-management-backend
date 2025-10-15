using System;
using System.Collections.Generic;

namespace IdentityService.Entities;

public partial class VehicleTransferOrder
{
    public Guid VehicleTransferOrderId { get; set; }

    public Guid? FromDealerId { get; set; }

    public Guid? ToDealerId { get; set; }

    public Guid? VehicleVersionId { get; set; }

    public int? Quantity { get; set; }

    public DateOnly? RequestDate { get; set; }

    public string? Status { get; set; }

    public virtual Dealer? FromDealer { get; set; }

    public virtual Dealer? ToDealer { get; set; }

    public virtual VehicleVersion? VehicleVersion { get; set; }
}
