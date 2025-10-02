namespace DealerService.DTOs.Responses.DealerTargetDTOs
{
    public class DealerTargetResponse
    {
        public Guid DealerTargetId { get; set; }
        public Guid DealerId { get; set; }
        public string DealerName { get; set; } = null!;
        public string Period { get; set; } = null!;
        public decimal TargetAmount { get; set; }
        public decimal AchievedAmount { get; set; }
        public DateTime StartDate { get; set; }
    }
}
