using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SENG302Template.Api.Models.Entities;
using SENG302Template.Api.Services;
using Shouldly;

namespace SENG302Template.Api.Tests.Integration.Services;

public class BookServiceTest : BaseIntegrationTestFixture
{
    private IBookService ServiceUnderTest => ServiceProvider.GetRequiredService<IBookService>();

    public BookServiceTest(WebApplicationFactory<Program> webAppFactory)
        : base(webAppFactory) { }

    [Fact]
    public async Task CreateNewBook_ValidDetails_BookInDatabase()
    {
        // Call the service method we are testing, which should add a new book to the database
        await ServiceUnderTest.CreateNewBookAsync("My New Book", "Mr Tester", 2025);
        
        // Check that the book has now been added to the database
        await using var context = await DbContextFactory.CreateDbContextAsync();

        var singleItemInDb = context.Books.ShouldHaveSingleItem();
        singleItemInDb.Id.ShouldBe(1);
        singleItemInDb.Title.ShouldBe("My New Book");
        singleItemInDb.Author.ShouldBe("Mr Tester");
        singleItemInDb.Year.ShouldBe(2025);
        
        // When checking the created time, use our mocked TestNow time (see BaseIntegrationTestFixture.TestNow)
        singleItemInDb.Created.ShouldBe(TestNow);   
    }

    [Fact]
    public async Task GetAllBooks_NoBooksInDb_EmptyCollectionReturned()
    {
        var returnedBooks = await ServiceUnderTest.GetAllBooks();
        returnedBooks.ShouldBeEmpty();
    }
    
    // We can parametrize our tests using [Theory] instead of fact, allowing us to run the same test
    // multiple times with different parameters 
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(50)]
    public async Task GetAllEntities_MultipleEntitiesInDb_CollectionOfSizeReturned(int bookCount)
    {
        // Create a list of however many entities we are told by bookCount
        var seedBooks = new List<Book>();
        for (var i = 0; i < bookCount; i++)
        {
            seedBooks.Add(new Book
            {
                Title = $"My Book {i}",
                Author = $"Author {i}",
                Year = 1950 + i,
                Created = TestNow,
            });
        }
        
        // Add those books to the database manually. (There is a service method to add a book,
        // but we only want to test the GetAllBooks method here, so we add test data manually).
        await using var context = await DbContextFactory.CreateDbContextAsync();
        context.Books.AddRange(seedBooks);
        await context.SaveChangesAsync();
        
        // Check that we receive the same number of books back as we just added to the database
        var returnedBooks = await ServiceUnderTest.GetAllBooks();
        returnedBooks.Count().ShouldBe(bookCount);
    }
}