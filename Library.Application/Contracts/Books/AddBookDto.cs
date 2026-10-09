using System.ComponentModel.DataAnnotations;

namespace Library.Application.Contracts.Books;

public class AddBookDto
{
    [Required, MinLength(1)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    public int Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(1, int.MaxValue)]
    public int Pages { get; set; }

    public DateTime PublicationYear { get; set; }

    [Required, MinLength(1)]
    public string Author { get; set; } = string.Empty;

    public int BookTypeId { get; set; }
}
