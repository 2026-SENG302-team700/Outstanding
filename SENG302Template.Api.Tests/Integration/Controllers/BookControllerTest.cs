using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SENG302Template.Api.Models.Entities;
using Shouldly;

namespace SENG302Template.Api.Tests.Integration.Controllers;

public class BookControllerTest : BaseIntegrationTestFixture
{
    public BookControllerTest(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory)
    {
    }
    
    // Adds sample books to the database, and returns these books for testing.
    private async Task<IEnumerable<Book>> AddSampleBooksToDatabase(int countOfBooksInDatabase)
    {
        var books = new List<Book>();
        for (var i = 0; i < countOfBooksInDatabase; i++)
        {
            books.Add(new Book
            {
                Title = $"Test Book {i}",
                Author = $"Author {i}",
                Year = 2000
            });
        }
        var dbContext = await DbContextFactory.CreateDbContextAsync();

        dbContext.Books.AddRange(books);
        await dbContext.SaveChangesAsync();
        
        return books;
    }

    [Fact]
    public async Task GetAllBooks_NoBooksExist_EmptyListReturned()
    {
        // We use the HttpClient from the base class which is already set up
        var books = await HttpClient.GetAsync("/api/Books");
        
        // We'll check the response code. This is the MINIMUM you should check, make sure you also check the actual response content!
        books.IsSuccessStatusCode.ShouldBe(true);
        
        // Now we check what was actually in the response.
        // First, deserialize the response content to the type we expect (IEnumerable<Book>)
        var booksReturned = await books.Content.ReadFromJsonAsync<IEnumerable<Book>>();

        // Then, assert that it is empty
        booksReturned.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task GetAllBooks_BooksExist_CorrectNumberOfBooksReturned(int countOfBooksInDatabase)
    {
        // Add as many books to the database as we specify in the test theory data
        var booksInDatabase = await AddSampleBooksToDatabase(countOfBooksInDatabase);
        
        // Now we query the API to get the books, and check that it returns what we expect (i.e., what we just added to the database)
        var books = await HttpClient.GetAsync("/api/Books");
        
        // Basic check that the status code is what we expect
        books.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        // Now we check the actual content
        var booksReturned = await books.Content.ReadFromJsonAsync<ICollection<Book>>();
        
        // First a simple check that we got the right number of books
        booksReturned!.Count.ShouldBe(countOfBooksInDatabase);
        
        // Now a more complex check, this time checking that the titles are what we expect
        var expectedBookTitles = booksInDatabase.Select(x => x.Title);
        var actualBookTitles = booksReturned.Select(x => x.Title);
        actualBookTitles.ShouldBeEquivalentTo(expectedBookTitles);
    }
}