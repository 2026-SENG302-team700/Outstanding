using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;
using Microsoft.EntityFrameworkCore;

namespace SENG302.Api.Tests.Integration.ControllerTests;

public class UserControllerTests : BaseIntegrationTestFixture
{
    public UserControllerTests(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

    private IUserService ServiceUnderTest => ServiceProvider.GetRequiredService<IUserService>();

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
    
    [Fact]
    public async Task InitiateOneTimeCode_ValidEmail_UpdatesDatabaseAndReturnsOk()
    {
        await AddTestUser();
        var request = new { Email = "test@example.com" };

        var response = await HttpClient.PutAsJsonAsync("/api/user/password/code/generation", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        await using var verifyContext = DbContextFactory.CreateDbContext();
        var updatedUser = await verifyContext.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        
        updatedUser.ShouldNotBeNull();
        updatedUser.OneTimeCode.ShouldNotBeNull();
        updatedUser.OneTimeCode.Length.ShouldBe(6);
    }
    
    [Fact]
    public async Task InitiateOneTimeCode_UserNotFound_ReturnsProblem()
    {
        var request = new { Email = "nonexistent@example.com" };

        var response = await HttpClient.PutAsJsonAsync("/api/user/password/code/generation", request);
        
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }
    
    [Fact]
    public async Task ValidateOneTimeCode_CorrectCode_ReturnsOk()
    {
        var email = "test@example.com";
        var secretCode = "123456";
        
        await using (var context = DbContextFactory.CreateDbContext())
        {
            context.Users.Add(new User
            {
                Email = email,
                DisplayName = "ValidateUser",
                PasswordKey = "password",
                Country = "NZ",
                OneTimeCode = secretCode
            });
            await context.SaveChangesAsync();
        }
        var request = new { Email = email, Code = secretCode };

        var response = await HttpClient.PostAsJsonAsync("/api/user/password/code/validation", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task ValidateOneTimeCode_WrongCode_ReturnsBadRequest()
    {
        var email = "test@example.com";
        await using (var context = DbContextFactory.CreateDbContext())
        {
            context.Users.Add(new User
            {
                Email = email,
                DisplayName = "ValidateUser",
                PasswordKey = "password",
                Country = "NZ",
                OneTimeCode = "111111" // stored code
            });
            await context.SaveChangesAsync();
        }

        var request = new { Email = email, Code = "222222" }; // wrong code

        var response = await HttpClient.PostAsJsonAsync("/api/user/password/code/validation", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task ValidateOneTimeCode_EmailNotFound_ReturnsNotFound()
    {
        var request = new { Email = "missing@test.com", Code = "123456" };

        var response = await HttpClient.PostAsJsonAsync("/api/user/password/code/validation", request);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}