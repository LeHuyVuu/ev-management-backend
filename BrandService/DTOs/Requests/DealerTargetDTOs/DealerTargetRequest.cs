using System.ComponentModel.DataAnnotations;

namespace BrandService.DTOs.Requests.DealerTargetDTOs
{
    public class DealerTargetRequest
    {
        [Required(ErrorMessage = "Period is required.")]
        [RegularExpression("^(Monthly|Quarterly|Yearly)$",
            ErrorMessage = "Period must be \"Monthly\", \"Quarterly\", or \"Yearly\".")]
        public string Period { get; set; } = null!;

        [Required(ErrorMessage = "TargetAmount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "TargetAmount must be greater than 0.")]
        public decimal TargetAmount { get; set; }

        [Required(ErrorMessage = "StartDate is required.")]
        public DateTime StartDate { get; set; }
    }
}
