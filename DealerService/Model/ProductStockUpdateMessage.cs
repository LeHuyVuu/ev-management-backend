
namespace ProductService.Model;

public class ProductStockUpdateMessage
{
    public int ProductId { get; set; }
    public int QuantityUpdated { get; set; }
}