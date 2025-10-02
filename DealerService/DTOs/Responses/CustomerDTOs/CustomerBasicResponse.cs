namespace ProductService.DTOs;

public class CustomerBasicResponse
{
    public Guid CustomerId {get; set;}
    public string Name {get; set;}
    public string Phone {get; set;}
    public string status {get; set;}
    public string? Address { get; set; }
    public string StaffContact { get; set; }
}