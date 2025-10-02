namespace CustomerService.DTOs.Responses.CustomerDTOs;

public class CustomerDetailResponse
{
    public string Name {get; set;}
    public string Email {get; set;}
    public string Phone {get; set;}
    public string Address {get; set;}
    public string Status {get; set;}
    public DateOnly LastInteractionDate {get; set;}
}