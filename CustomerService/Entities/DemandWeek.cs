using System;
using System.Collections.Generic;

namespace CustomerService.Entities;

public partial class DemandWeek
{
    public Guid? DealerId { get; set; }

    public Guid? VehicleVersionId { get; set; }

    public DateOnly? WeekStart { get; set; }

    public int? YContractsWeek { get; set; }
}
