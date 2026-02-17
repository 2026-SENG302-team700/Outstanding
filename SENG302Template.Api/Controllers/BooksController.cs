using Microsoft.AspNetCore.Mvc;
using SENG302Template.Api.Models.Entities;
using SENG302Template.Api.Services;

namespace SENG302Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
  private readonly IBookService _bookService;

  public BooksController(IBookService bookService)
  {
    _bookService = bookService;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
  {
    var books = await _bookService.GetAllBooks();
    return Ok(books);
  }

  [HttpGet("{id:int}")]
  public async Task<ActionResult<Book>> GetBook(int id)
  {
    var book = await _bookService.GetBookByIdAsync(id);
    if (book == null)
      return NotFound();

    return Ok(book);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<ActionResult<Book>> CreateBook([FromBody] Book book)
  {
    if (string.IsNullOrWhiteSpace(book.Title) ||
        string.IsNullOrWhiteSpace(book.Author) ||
        book.Year <= 0)
    {
      return BadRequest("Invalid book data");
    }

    var newBook = await _bookService.CreateNewBookAsync(book.Title, book.Author, book.Year);
    return CreatedAtAction(nameof(GetBook), new { id = newBook.Id }, newBook);
  }

  [HttpDelete("{id:int}")]
  [ValidateAntiForgeryToken]
  public async Task<ActionResult> DeleteBook(int id)
  {
    var result = await _bookService.DeleteBookAsync(id);
    if (!result)
      return NotFound();

    return NoContent();
  }
}
