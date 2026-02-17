using SENG302Template.Api.DataAccess;
using SENG302Template.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace SENG302Template.Api.Services;

public interface IBookService
{
    Task<Book> CreateNewBookAsync(string name, string author, int year);
    Task<IEnumerable<Book>> GetAllBooks();
    Task<Book?> GetBookByIdAsync(int id);
    Task<bool> DeleteBookAsync(int id);
}
public class BookService : IBookService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly TimeProvider _timeProvider;

    public BookService(IDbContextFactory<DatabaseContext> dbContextFactory, TimeProvider timeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _timeProvider = timeProvider;
    }
    
    
    public async Task<Book> CreateNewBookAsync(string name, string author, int year)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var book = new Book
        {
            // Because ID is marked as the [Key], it will be automatically generated for us when we save to the DB
            Title = name,
            Author = author,
            Year = year,
            // Instead of using DateTime.Now (which we can't mock in testing), we use a TimeProvider (which we can mock)
            Created = _timeProvider.GetUtcNow()
        };

        context.Add(book);
        await context.SaveChangesAsync();

        return book;
    }

    public async Task<IEnumerable<Book>> GetAllBooks()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        return await context.Books.ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        return await context.Books.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var book = await context.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book == null)
        {
            return false;
        }

        context.Books.Remove(book);
        await context.SaveChangesAsync();

        return true;
    }
}