namespace BrandService.DTOs.Requests.DealerDTOs
{
    public class DealerRequest
    {
        public string? DealerCode { get; set; }
        public string Name { get; set; } = null!;
        public string? Region { get; set; }
        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string Status { get; set; } = null!;

    }
}
