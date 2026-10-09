namespace Library.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int Price { get; set; }
    public DateTime OrderDate { get; set; }
    public string? DeliveryType { get; set; }
    public string? PaymentType { get; set; }
    public string? Note { get; set; }
    public string? Address { get; set; }

    public ICollection<ClientOrderBridge> ClientOrderBridges { get; set; } = new List<ClientOrderBridge>();
}
