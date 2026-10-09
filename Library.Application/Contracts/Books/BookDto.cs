namespace Library.Application.Contracts.Books;

public class BookDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Price { get; set; }
    public int Quantity { get; set; }
    public int Pages { get; set; }
    public DateTime PublicationYear { get; set; }
    public string? Author { get; set; }
    public int BookTypeId { get; set; }
    public string? BookTypeName { get; set; }



}
