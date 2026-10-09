namespace Library.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Price { get; set; }
    public int Quantity { get; set; }
    public int Pages { get; set; }
    public DateTime PublicationYear { get; set; }
    public string Author { get; set; } = string.Empty;
    public long? Isbn { get; set; }

    public BookBookTypeBridge? BookBookType { get; set; }

    public ICollection<ClientOrderBridge> ClientOrderBridges { get; set; } = new List<ClientOrderBridge>();
}