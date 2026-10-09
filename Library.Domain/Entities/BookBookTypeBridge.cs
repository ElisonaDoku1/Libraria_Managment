namespace Library.Domain.Entities;

public class BookBookTypeBridge
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book? Book { get; set; }

    public int BookTypeId { get; set; }
    public BookType? BookType { get; set; }
}