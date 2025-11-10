using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UtilityService.Entities;

[Keyless]
public partial class simple_forecast
{
    public DateOnly? dt { get; set; }

    public Guid? vehicle_version_id { get; set; }

    [StringLength(100)]
    public string? region { get; set; }

    [Precision(18, 2)]
    public decimal? p50 { get; set; }

    [Precision(18, 2)]
    public decimal? p90 { get; set; }

    public DateTime? created_at { get; set; }
}
