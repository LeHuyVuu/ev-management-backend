using System;
using System.Collections.Generic;

namespace CustomerService.Entities;

public partial class SimpleForecast
{
    public DateOnly? Dt { get; set; }

    public Guid? VehicleVersionId { get; set; }

    public string? Region { get; set; }

    public decimal? P50 { get; set; }

    public decimal? P90 { get; set; }

    public DateTime? CreatedAt { get; set; }
}
