using Library.Application.Contracts.Books;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagmentSys.Controllers;

[ApiController]
[Authorize]
[Route("api/Book")]
public class BookController : ControllerBase
{
    private readonly IBookService _bookService;


    public BookController(IBookService bookService) => _bookService = bookService;


    [Authorize(Roles = "Admin,Staff")]
    [HttpPost("AddBook")]
    public async Task<ActionResult<BookDto>> AddBook([FromBody] AddBookDto dto, CancellationToken ct)
        => Ok(await _bookService.AddBookAsync(dto, ct));




    [HttpGet("GetAllBooks")]
    //collection of BookDto objects
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks(CancellationToken ct)
        => Ok(await _bookService.GetAllBooksAsync(ct));





    [HttpGet("GetBookById/{id:int}")]
    public async Task<ActionResult<BookDto>> GetBookById(int id, CancellationToken ct)
    {
        var book = await _bookService.GetBookByIdAsync(id, ct);
        return book is null ? NotFound() : Ok(book);
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("EditBook")]
    public async Task<ActionResult<BookDto>> EditBook([FromBody] EditBookDto dto, CancellationToken ct)
        => Ok(await _bookService.EditBookAsync(dto, ct));



    [Authorize(Roles = "Admin,Staff")]
    [HttpDelete("DeleteBook/{id:int}")]
    public async Task<IActionResult> DeleteBook(int id, CancellationToken ct)
    {
        await _bookService.DeleteBookAsync(id, ct);

        return Ok();
    }



    [Authorize(Roles = "Admin,Staff")]
    [HttpPost("AddBookType")]
    public async Task<ActionResult<BookTypeDto>> AddBookType([FromBody] AddBookTypeDto dto, CancellationToken ct)
        => Ok(await _bookService.AddBookTypeAsync(dto, ct));

    
    
    
    [HttpGet("GetBookTypes")]
    public async Task<ActionResult<IEnumerable<BookTypeDto>>> GetBookTypes(CancellationToken ct)
        => Ok(await _bookService.GetBookTypesAsync(ct));

    
    
    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("EditBooKType")]
    public async Task<ActionResult<BookTypeDto>> EditBookType([FromBody] BookTypeDto dto, CancellationToken ct)
        => Ok(await _bookService.EditBookTypeAsync(dto, ct));

    
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("DeleteBookType/{id:int}")]
    public async Task<IActionResult> DeleteBookType(int id, CancellationToken ct)
    {
        await _bookService.DeleteBookTypeAsync(id, ct);
        return Ok();
    }






}
