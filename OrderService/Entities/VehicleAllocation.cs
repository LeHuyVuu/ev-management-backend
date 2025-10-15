    using System;
    using System.Collections.Generic;

    namespace OrderService.Entities;

    public partial class VehicleAllocation
    {
        public Guid AllocationId { get; set; }

        public Guid DealerId { get; set; }

        public Guid VehicleVersionId { get; set; }

        public int Quantity { get; set; }

        public DateOnly RequestDate { get; set; }

        public DateOnly? ExpectedDelivery { get; set; }

        public string Status { get; set; } = null!;

        public virtual Dealer Dealer { get; set; } = null!;

        public virtual VehicleVersion VehicleVersion { get; set; } = null!;
    }
