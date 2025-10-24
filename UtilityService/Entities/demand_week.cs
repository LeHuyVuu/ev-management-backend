using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UtilityService.Entities;

[Keyless]
public partial class demand_week
{
    public Guid? dealer_id { get; set; }

    public Guid? vehicle_version_id { get; set; }

    public DateOnly? week_start { get; set; }

    public int? y_contracts_week { get; set; }
}
