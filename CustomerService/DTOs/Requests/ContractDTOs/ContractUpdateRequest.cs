using System.Text.Json.Serialization;

namespace CustomerService.DTOs.Requests.ContractDTOs;

public class ContractUpdateRequest
{
    public string PaymentMethod { get; set; } = null!;
    public int Months { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Pct { get; set; }
    public decimal NewAmountPaid { get; set; }
}