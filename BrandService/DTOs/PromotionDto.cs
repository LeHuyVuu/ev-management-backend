namespace BrandService.DTOs
{
    public class PromotionDto
    {
        public class PromotionRequest
        {
            public string? Name { get; set; }
            public string? Type { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? Status { get; set; }
            public string? Description { get; set; }
        }

        public class PromotionResponse
        {
            public Guid PromotionId { get; set; }
            public string Name { get; set; } = null!;
            public string Type { get; set; } = null!;
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Status { get; set; } = null!;
            public string? Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

    }
}
