using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("test_drives", Schema = "evm")]
public partial class test_drife
{
    [Key]
    public Guid test_drive_id { get; set; }

    public Guid dealer_id { get; set; }

    public Guid customer_id { get; set; }

    public Guid vehicle_version_id { get; set; }

    public Guid? staff_user_id { get; set; }

    public DateOnly drive_date { get; set; }

    [StringLength(50)]
    public string time_slot { get; set; } = null!;

    public bool confirm_sms { get; set; }

    public bool confirm_email { get; set; }

    [StringLength(30)]
    public string status { get; set; } = null!;

    [ForeignKey("customer_id")]
    [InverseProperty("test_drives")]
    public virtual customer customer { get; set; } = null!;

    [ForeignKey("dealer_id")]
    [InverseProperty("test_drives")]
    public virtual dealer dealer { get; set; } = null!;

    [ForeignKey("staff_user_id")]
    [InverseProperty("test_drives")]
    public virtual user? staff_user { get; set; }

    [ForeignKey("vehicle_version_id")]
    [InverseProperty("test_drives")]
    public virtual vehicle_version vehicle_version { get; set; } = null!;
}
