using Library.Application.Common;
using Library.Application.Contracts.Books;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Services;

public class BookService : IBookService
{
    private readonly LibraryDbContext _context;
    public BookService(LibraryDbContext context) => _context = context;

    public async Task<BookDto> AddBookAsync(AddBookDto dto, CancellationToken ct = default)
    {
        var bookType = await _context.BookTypes.FindAsync([dto.BookTypeId], ct)
            ?? throw new NotFoundException($"Book type {dto.BookTypeId} was not found.");


        var book = new Book
        {

            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            Quantity = dto.Quantity,
            Pages = dto.Pages,
            PublicationYear = dto.PublicationYear,
            Author = dto.Author,

            // linked through the navigation, so EF fills in BookId itself and saves book + link in one transaction
            BookBookType = new BookBookTypeBridge { BookTypeId = bookType.Id },

        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync(ct);

        return ToDto(book, bookType.BookTypeName);

    }

    public async Task<IReadOnlyList<BookDto>> GetAllBooksAsync(CancellationToken ct = default)
    {

        return await _context.Books
            .AsNoTracking()
            // pulls in BookBookType + BookType too, in the same trip, so BookTypeName below isn't null
            .Include(b => b.BookBookType!).ThenInclude(bb => bb.BookType)
            .OrderBy(b => b.Id)
            .Select(b => new BookDto
            {

                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                Price = b.Price,
                Quantity = b.Quantity,
                Pages = b.Pages,
                PublicationYear = b.PublicationYear,
                Author = b.Author,
                BookTypeId = b.BookBookType != null ? b.BookBookType.BookTypeId : 0,
                BookTypeName = b.BookBookType != null ? b.BookBookType.BookType!.BookTypeName : null,
            })

            .ToListAsync(ct);
    }

    public async Task<BookDto?> GetBookByIdAsync(int id, CancellationToken ct = default)
    {
        var book = await _context.Books
            .AsNoTracking()
            // pulls in BookBookType + BookType too, in the same trip, so BookTypeName below isn't null
            .Include(b => b.BookBookType!).ThenInclude(bb => bb.BookType)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return book is null ? null : ToDto(book, book.BookBookType?.BookType?.BookTypeName);
    }



    public async Task<BookDto> EditBookAsync(EditBookDto dto, CancellationToken ct = default)
    {
        var book = await _context.Books
            .Include(b => b.BookBookType)
            .FirstOrDefaultAsync(b => b.Id == dto.Id, ct)
            ?? throw new NotFoundException($"Book {dto.Id} was not found.");

        var bookType = await _context.BookTypes.FindAsync([dto.BookTypeId], ct)
            ?? throw new NotFoundException($"Book type {dto.BookTypeId} was not found.");

        book.Title = dto.Title ?? book.Title;
        book.Description = dto.Description;
        book.Price = dto.Price;
        book.Quantity = dto.Quantity;
        book.Pages = dto.Pages;
        book.PublicationYear = dto.PublicationYear;
        book.Author = dto.Author ?? book.Author;

        if (book.BookBookType is null)
            _context.BookBookTypeBridges.Add(new BookBookTypeBridge { BookId = book.Id, BookTypeId = bookType.Id });
        else
            book.BookBookType.BookTypeId = bookType.Id;

        await _context.SaveChangesAsync(ct);
        return ToDto(book, bookType.BookTypeName);
    }

    public async Task DeleteBookAsync(int id, CancellationToken ct = default)
    {

        var book = await _context.Books
            .Include(b => b.BookBookType)
            .Include(b => b.ClientOrderBridges)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new NotFoundException($"Book {id} was not found.");

        if (book.ClientOrderBridges.Count > 0)
            throw new ConflictException("Cannot delete a book that has existing orders.");

        if (book.BookBookType is not null)
            _context.BookBookTypeBridges.Remove(book.BookBookType);

        _context.Books.Remove(book);
        await _context.SaveChangesAsync(ct);

    }


    public async Task<BookTypeDto> AddBookTypeAsync(AddBookTypeDto dto, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(dto.BookTypeName) &&
            await _context.BookTypes.AnyAsync(t => t.BookTypeName == dto.BookTypeName, ct))
            throw new ConflictException($"Book type '{dto.BookTypeName}' already exists.");

        var bookType = new BookType { BookTypeName = dto.BookTypeName };

        _context.BookTypes.Add(bookType);
        await _context.SaveChangesAsync(ct);

        return new BookTypeDto { Id = bookType.Id, BookTypeName = bookType.BookTypeName };

    }


    public async Task<IReadOnlyList<BookTypeDto>> GetBookTypesAsync(CancellationToken ct = default)
    {

        return await _context.BookTypes
            .AsNoTracking()
            .OrderBy(t => t.Id)
            //for each row (I'm calling it t here), build a new BookTypeDto using that row's
            .Select(t => new BookTypeDto { Id = t.Id, BookTypeName = t.BookTypeName })
            .ToListAsync(ct);

    }


    public async Task<BookTypeDto> EditBookTypeAsync(BookTypeDto dto, CancellationToken ct = default)
    {
        var bookType = await _context.BookTypes.FindAsync([dto.Id], ct)
            ?? throw new NotFoundException($"Book type {dto.Id} was not found.");

        if (!string.IsNullOrWhiteSpace(dto.BookTypeName) &&
            await _context.BookTypes.AnyAsync(t => t.Id != dto.Id && t.BookTypeName == dto.BookTypeName, ct))
            throw new ConflictException($"Book type '{dto.BookTypeName}' already exists.");

        bookType.BookTypeName = dto.BookTypeName;
        await _context.SaveChangesAsync(ct);


        return new BookTypeDto { Id = bookType.Id, BookTypeName = bookType.BookTypeName };
    }



    public async Task DeleteBookTypeAsync(int id, CancellationToken ct = default)
    {

        var bookType = await _context.BookTypes.FindAsync([id], ct)
            ?? throw new NotFoundException($"Book type {id} was not found.");

        var inUse = await _context.BookBookTypeBridges.AnyAsync(b => b.BookTypeId == id, ct);
        if (inUse)
            throw new ConflictException("Cannot delete a book type that is still assigned to books.");

        _context.BookTypes.Remove(bookType);

        await _context.SaveChangesAsync(ct);
    }



                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
    private static BookDto ToDto(Book book, string? bookTypeName) => new()
    {

        Id = book.Id,
        Title = book.Title,
        Description = book.Description,
        Price = book.Price,
        Quantity = book.Quantity,
        Pages = book.Pages,
        PublicationYear = book.PublicationYear,
        Author = book.Author,
        BookTypeId = book.BookBookType?.BookTypeId ?? 0,
        BookTypeName = bookTypeName,

    };

      

}
