using System;
using System.Collections.Generic;

namespace CustomerService.Entities;

public partial class VDemandHist
{
    public DateOnly? Dt { get; set; }

    public string? Region { get; set; }

    public Guid? VehicleVersionId { get; set; }

    public decimal? DemandUnits { get; set; }
}
