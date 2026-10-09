namespace Library.Application.Contracts.Orders;

public class EditOrderDto
{
    public int OrderId { get; set; }
    public int BookId { get; set; }
    public int ClientId { get; set; }
    public string? DeliveryType { get; set; }
    public string? PaymentType { get; set; }
    public string? Note { get; set; }
}
