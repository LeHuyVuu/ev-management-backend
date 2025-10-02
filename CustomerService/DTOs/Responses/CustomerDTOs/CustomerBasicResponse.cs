namespace CustomerService.DTOs.Responses.CustomerDTOs;

public class CustomerBasicResponse
{
    public Guid CustomerId {get; set;}
    public string Name {get; set;}
    public string Phone {get; set;}
    public string status {get; set;}
}