namespace DealerService.DTOs.Responses.DealerDTOs
{
    public class DealerResponse
    {
        public Guid DealerId { get; set; }
        public string DealerCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Region { get; set; }
        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string Status { get; set; } = null!;
    }
}
