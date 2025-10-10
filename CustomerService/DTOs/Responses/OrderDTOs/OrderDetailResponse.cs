namespace CustomerService.DTOs.Responses.OrderDTOs;

public class OrderDetailResponse
{
    public Guid OrderId { get; set; }
    
    // Thông tin khách hàng
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email {  get; set; }
    
    // Thông tin xe
    public string Brand { get; set; }
    public string ModelName { get; set; }
    public string Color {  get; set; }
    
    // Thông tin đơn hàng
    public string DeliveryAddress { get; set; }
    public DateOnly? DeliveryDate { get; set; }
    public string Status { get; set; }
}