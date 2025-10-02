using System.ComponentModel.DataAnnotations;

namespace BrandService.DTOs.Requests.DealerDTOs
{
    public class DealerRequest
    {
        [Required(ErrorMessage = "DealerCode is required.")]
        public string DealerCode { get; set; } = null!;

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = null!;
        public string? Region { get; set; }
        public string? Address { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(active|expired|terminated)$",
            ErrorMessage = "Status must be \"active\", \"expired\", or \"terminated\".")]
        public string Status { get; set; } = null!;

    }
}
