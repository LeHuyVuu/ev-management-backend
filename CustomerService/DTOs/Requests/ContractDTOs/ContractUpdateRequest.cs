namespace CustomerService.DTOs.Requests.ContractDTOs;

public class ContractUpdateRequest
{
    public string PaymentMethod { get; set; } = null!;
    public int Months { get; set; }
    public int Pct { get; set; }
    public decimal NewAmountPaid { get; set; }
}