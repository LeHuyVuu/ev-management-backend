using System;
using System.Collections.Generic;

namespace CustomerService.Entities;

public partial class SimpleForecastsState
{
    public short Id { get; set; }

    public DateTime? LastRefreshed { get; set; }

    public long? ContractsCnt { get; set; }

    public long? ContractsSig { get; set; }

    public long? QuotesCnt { get; set; }

    public long? QuotesSig { get; set; }

    public long? DiscountsCnt { get; set; }

    public long? DiscountsSig { get; set; }

    public long? PromotionsCnt { get; set; }

    public long? PromotionsSig { get; set; }

    public long? InventoryCnt { get; set; }

    public long? InventorySig { get; set; }
}
