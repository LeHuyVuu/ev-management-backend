using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UtilityService.Entities;

[Keyless]
public partial class v_demand_hist
{
    public DateOnly? dt { get; set; }

    [StringLength(100)]
    public string? region { get; set; }

    public Guid? vehicle_version_id { get; set; }

    public decimal? demand_units { get; set; }
}
