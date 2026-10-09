namespace Library.Application.Contracts.Orders;

public class OrderItemDto
{
    public int OrderId { get; set; }
    public int Price { get; set; }
    public DateTime OrderDate { get; set; }
    public string? DeliveryType { get; set; }
    public string? PaymentType { get; set; }
    public string? Note { get; set; }
    public string? Address { get; set; }

    public int ClientOrderBridgeId { get; set; }
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientEmail { get; set; }

    public int BookId { get; set; }
    public string? BookTitle { get; set; }
    public string? BookAuthor { get; set; }
    public int BookPrice { get; set; }
    public int BookPages { get; set; }
    public DateTime BookPublicationYear { get; set; }
    public string? BookDescription { get; set; }
}
