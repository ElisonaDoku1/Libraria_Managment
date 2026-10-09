using Library.Application.Contracts.Books;

namespace Library.Application.Interfaces;

public interface IBookService
{
    Task<BookDto> AddBookAsync(AddBookDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<BookDto>> GetAllBooksAsync(CancellationToken ct = default);
    Task<BookDto?> GetBookByIdAsync(int id, CancellationToken ct = default);
    Task<BookDto> EditBookAsync(EditBookDto dto, CancellationToken ct = default);
    Task DeleteBookAsync(int id, CancellationToken ct = default);

    Task<BookTypeDto> AddBookTypeAsync(AddBookTypeDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<BookTypeDto>> GetBookTypesAsync(CancellationToken ct = default);
    Task<BookTypeDto> EditBookTypeAsync(BookTypeDto dto, CancellationToken ct = default);
    Task DeleteBookTypeAsync(int id, CancellationToken ct = default);
}
