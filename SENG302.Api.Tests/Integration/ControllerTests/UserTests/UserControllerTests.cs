using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using Microsoft.EntityFrameworkCore;
using SENG302.Api.Controllers.UserController;

namespace SENG302.Api.Tests.Integration.ControllerTests;

public class UserControllerTests : BaseIntegrationTestFixture
{
    private readonly IEmailService _mockEmailService;
    private readonly IOneTimeCodeService _mockOneTimeCodeService;
    private readonly IFileService _mockFileService;
    private readonly UserController _controller;

    private IUserService ServiceUnderTest => ServiceProvider.GetRequiredService<IUserService>();
    public UserControllerTests(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory)
    {
        _mockOneTimeCodeService = Substitute.For<IOneTimeCodeService>();
        _mockEmailService = Substitute.For<IEmailService>();
        _mockFileService = Substitute.For<IFileService>();
        _controller =
            new UserController(ServiceUnderTest, _mockFileService, _mockOneTimeCodeService, _mockEmailService);
    }
    

    private async Task AddTestUser()
    {
        // Add user to be updated to db
        await using var context = DbContextFactory.CreateDbContext();
        context.Users.Add(new User
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        await context.SaveChangesAsync();
    }
    
    private void SetupUserContext(string userId, string name, string email)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Name, name), new Claim(ClaimTypes.Email, email) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
    
    

    
    
    private async Task SetupTestUser()
    {
        await AddTestUser();
        SetupUserContext("1", "Test User", "test@example.com");
    }

    [Fact]
    public async Task UpdateUser_Success_ReturnOk()
    {
        await AddTestUser();

        // Send update request
        var data = new
        {
            Email = "updated@example.com",
            DisplayName = "Updated User",
            Country = "US"
        };
        var response = await HttpClient.PutAsJsonAsync("/api/user", data);

        // Get updated context and verify
        await using var verifyContext = DbContextFactory.CreateDbContext();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedUser = await verifyContext.Users.FirstOrDefaultAsync(u => u.Id == 1);
        updatedUser!.Email.ShouldBe("updated@example.com");
        updatedUser.DisplayName.ShouldBe("Updated User");
        updatedUser.Country.ShouldBe("US");
    }

    [Theory]
    [InlineData("", "test", "NZ")]
    [InlineData("updated@example.com", "", "NZ")]
    [InlineData("updated@example.com", "test", "")]
    public async Task UpdateUser_MissingField_BadRequest(string newEmail, string newDisplayName, string newCountry)
    {
        await AddTestUser();

        // Send update request
        var data = new
        {
            Email = newEmail,
            DisplayName = newDisplayName,
            Country = newCountry
        };
        var response = await HttpClient.PutAsJsonAsync("/api/user", data);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("test.userexample.com")]      // Missing @ symbol
    [InlineData("@example.com")]              // Missing username
    [InlineData("test.user@")]                // Missing domain
    [InlineData("test.user@example")]         // Missing top-level domain (TLD)
    [InlineData("t st.user@example.com")]     // Spaces
    [InlineData("te..st.user@example.com")]   // Consecutive periods
    [InlineData("test@user@example.com")]     // Multiple @ symbols
    [InlineData("test.user@gmail,com")]       // Missing dot in domain
    public async Task UpdateUser_MalformedEmail_BadRequest(string newEmail)
    {
        await AddTestUser();

        // Send update request
        var data = new
        {
            Email = newEmail,
            DisplayName = "Test",
            Country = "NZ"
        };
        var response = await HttpClient.PutAsJsonAsync("/api/user", data);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_EmailAlreadyExists_BadRequest()
    {
        await AddTestUser();

        // Add user to be updated to db
        await using var context = DbContextFactory.CreateDbContext();
        context.Users.Add(new User
        {
            Id = 2,
            Email = "update@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        await context.SaveChangesAsync();

        // Send update request
        var data = new
        {
            Email = "update@example.com",
            DisplayName = "Test",
            Country = "NZ"
        };
        var response = await HttpClient.PutAsJsonAsync("/api/user", data);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("Hi")]
    [InlineData("Hi!")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // 65 'a's
    public async Task UpdateUser_InvalidDisplayName_BadRequest(string newDisplayName)
    {
        await AddTestUser();

        // Send update request
        var data = new
        {
            Email = "update@example.com",
            DisplayName = newDisplayName,
            Country = "NZ"
        };
        var response = await HttpClient.PutAsJsonAsync("/api/user", data);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

    }
    
    

    

    private IFormFile GetMockFile(string filename, Byte[] content, string mimeType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, stream.Length, "file", filename)
        {
            Headers = new HeaderDictionary(),
            ContentType = mimeType
        };
    }
}