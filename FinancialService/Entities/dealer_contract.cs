using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Entities;

[Table("dealer_contracts", Schema = "evm")]
public partial class dealer_contract
{
    [Key]
    public Guid dealer_contract_id { get; set; }

    public Guid dealer_id { get; set; }

    public DateOnly start_date { get; set; }

    public DateOnly end_date { get; set; }

    [StringLength(30)]
    public string status { get; set; } = null!;

    [ForeignKey("dealer_id")]
    [InverseProperty("dealer_contracts")]
    public virtual dealer dealer { get; set; } = null!;
}
