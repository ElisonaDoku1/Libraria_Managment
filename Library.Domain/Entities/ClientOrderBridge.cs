namespace Library.Domain.Entities;

public class ClientOrderBridge
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book? Book { get; set; }

    public int ClientId { get; set; }
    public Client? Client { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }
}
