namespace BrandService.DTOs
{
    public class DealerTargetDTO
    {
        public Guid DealerTargetId { get; set; }
        public Guid DealerId { get; set; }
        public string Period { get; set; } = null!; // monthly, quarterly, yearly
        public decimal TargetAmount { get; set; }
        public decimal AchievedAmount { get; set; }
        public DateTime StartDate { get; set; }
    }
}
