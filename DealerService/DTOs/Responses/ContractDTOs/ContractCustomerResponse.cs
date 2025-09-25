namespace ProductService.DTOs;

public class ContractCustomerResponse
{
    public Guid ContractId { get; set; }

    public string Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}