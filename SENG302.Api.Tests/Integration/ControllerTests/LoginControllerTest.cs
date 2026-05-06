using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SENG302.Api.Models.Entities;
using Shouldly;
using Microsoft.AspNetCore.Identity;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace SENG302.Api.Tests.Integration.ControllerTests;

public class LoginControllerTest : BaseIntegrationTestFixture
{
    public LoginControllerTest(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

    [Theory]
    [InlineData("shiv.sheep@gmail.com", "ShivSheep", "fella!1Aa", "", "ES")]
    [InlineData("swag.mint@gmail.com", "SwagMintt", "Sheeep$1a", "Sheep", "NZ")]
    [InlineData("trad.horse@gmail.com", "bob", "TradTrad$4a", "TradTrad1", "US")]
    [InlineData("steven@wilson.uk", "Porcupine", "TreeB0&a", "tree", "NZ")]
    public async Task LoginUser_IncorrectPasswords_ReturnUnauthorised(string userEmail, string userDisplayName, string passwordString, string otherPasswordString, string userCountry)
    {
        await using var context = DbContextFactory.CreateDbContext();
        PasswordHasher<User> passwordHasher = new();
        var user = new User
        {
            Id = 1,
            Email = userEmail,
            DisplayName = userDisplayName,
            Country = userCountry,
            EmailVerified = true,
            TimeCreated = DateTime.UtcNow
        };
        user.PasswordKey = passwordHasher.HashPassword(user, passwordString);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var message = await HttpClient.PostAsJsonAsync("/api/login", new
        {
            Email = userEmail,
            PasswordString = otherPasswordString
        });
        message.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var content = await message.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        json.GetProperty("login").GetBoolean().ShouldBe(false);
        json.GetProperty("message").GetString().ShouldBe("Invalid email or password");
        json.GetProperty("hashStatus").GetBoolean().ShouldBe(false);
    }

    [Theory]
    [InlineData("shiv.sheep@gmail.com", "ShivSheep", "fella!1Aa", "shivsheep@gmail.com", "ES")]
    [InlineData("swag.mint@gmail.com", "SwagMintt", "Sheeep$1a", "Sswag.mint@gmail.com", "NZ")]
    public async Task LoginUser_NonValidEmail_ReturnBadRequest(string userEmail, string userDisplayName, string passwordString, string fakeEmail, string userCountry)
    {
        var registerData = new
        {
            Email = userEmail,
            DisplayName = userDisplayName,
            PasswordString = passwordString,
            PasswordConfirm = passwordString,
            Country = userCountry
        };

        var loginData = new
        {
            Email = fakeEmail,
            // we use the same password here because these tests 
            // should fail based on an incorrect email, not password, 
            // it does not matter what password is used.
            PasswordString = passwordString
        };
        var register = await HttpClient.PostAsJsonAsync("/api/register", registerData);
        register.StatusCode.ShouldBe(HttpStatusCode.OK);

        var message = await HttpClient.PostAsJsonAsync("/api/login", loginData);
        message.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var content = await message.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        json.GetProperty("login").GetBoolean().ShouldBe(false);
        json.GetProperty("message").GetString().ShouldBe("Invalid email or password");
        json.GetProperty("hashStatus").GetBoolean().ShouldBe(false);
    }


    [Fact]
    public async Task LoginUser_CorrectLoginDetails_ReturnLoginSuccess()
    {
        await using var context = DbContextFactory.CreateDbContext();
        PasswordHasher<User> passwordHasher = new();
        var user = new User
        {
            Id = 1,
            Email = "jdev@dev.com",
            DisplayName = "JJ Devy",
            Country = "AU",
            EmailVerified = true,
            TimeCreated = DateTime.UtcNow
        };
        user.PasswordKey = passwordHasher.HashPassword(user, "c00lPasSw0rdont@ME");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var login = new
        {
            Email = "jdev@dev.com",
            PasswordString = "c00lPasSw0rdont@ME",
        };
        var response = await HttpClient.PostAsJsonAsync("/api/login", login);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        json.GetProperty("login").GetBoolean().ShouldBe(true);
        json.GetProperty("message").GetString().ShouldBe("JJ Devy");
    }

    [Fact]
    public async Task LoginUser_DifferentEmailCasing_ReturnLoginSuccess()
    {
        await using var context = DbContextFactory.CreateDbContext();
        PasswordHasher<User> passwordHasher = new();
        var user = new User
        {
            Id = 1,
            Email = "jdev@dev.com",
            DisplayName = "JJ Devy",
            Country = "AU",
            EmailVerified = true,
            TimeCreated = DateTime.UtcNow
        };
        user.PasswordKey = passwordHasher.HashPassword(user, "c00lPasSw0rdont@ME");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var login = new
        {
            Email = "JDev@deV.cOm",
            PasswordString = "c00lPasSw0rdont@ME",
        };
        var response = await HttpClient.PostAsJsonAsync("/api/login", login);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        json.GetProperty("login").GetBoolean().ShouldBe(true);
        json.GetProperty("message").GetString().ShouldBe("JJ Devy");
    }

    [Theory]
    [InlineData("shiv.sheep@gmail.com", "ShivSheep", "fella!1Aa", "shivsheep.gmail.com", "ES")]
    [InlineData("swag.mint@gmail.com", "SwagMintt", "Sheeep$1a", "2016swag", "NZ")]
    [InlineData("swag.mint@gmail.com", "SwagMintt", "Sheeep$1a", "shiv3.hear❤️t@gmail.com", "NZ")]
    public async Task LoginUser_MalformedEmail_ReturnBadRequest(
        string userEmail,
        string userDisplayName,
        string passwordString,
        string fakeEmail,
        string userCountry
        )
    {
        var registerData = new
        {
            Email = userEmail,
            DisplayName = userDisplayName,
            PasswordString = passwordString,
            PasswordConfirm = passwordString,
            Country = userCountry
        };

        var loginData = new
        {
            Email = fakeEmail,
            // we use the same password here because these tests 
            // should fail based on an incorrect email, not password, 
            // it does not matter what password is used.
            PasswordString = passwordString
        };
        var register = await HttpClient.PostAsJsonAsync("/api/register", registerData);
        register.StatusCode.ShouldBe(HttpStatusCode.OK);

        var message = await HttpClient.PostAsJsonAsync("/api/login", loginData);
        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var content = await message.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        json.GetProperty("login").GetBoolean().ShouldBe(false);
        json.GetProperty("message").GetString().ShouldBe(
            "Invalid email address. Email must be in the format 'jane@doe.nz'"
            );
        json.GetProperty("hashStatus").GetBoolean().ShouldBe(false);
    }

    [Fact]
    public async Task LoginUser_UnverifiedEmail_Unauthorized()
    {
        await using var context = DbContextFactory.CreateDbContext();
        PasswordHasher<User> passwordHasher = new();
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "test",
            Country = "NZ",
            EmailVerified = false,
            CodeGenerationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 20,
            TimeCreated = DateTime.UtcNow
        };
        user.PasswordKey = passwordHasher.HashPassword(user, "Team700!");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var message = await HttpClient.PostAsJsonAsync("/api/login", new
        {
            Email = "test@example.com",
            PasswordString = "Team700!"
        });
        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var content = await message.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        json.GetProperty("login").GetBoolean().ShouldBe(false);
        json.GetProperty("message").GetString().ShouldBe("Account is not validated yet, check your emails.");
    }


    [Fact]
    public async Task LoginUser_UnverifiedEmailTimedOut_Unauthorized()
    {
        await using var context = DbContextFactory.CreateDbContext();
        PasswordHasher<User> passwordHasher = new();
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "test",
            Country = "NZ",
            EmailVerified = false,
            CodeGenerationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 400,
            TimeCreated = DateTime.UtcNow
        };
        user.PasswordKey = passwordHasher.HashPassword(user, "Team700!");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var message = await HttpClient.PostAsJsonAsync("/api/login", new
        {
            Email = "test@example.com",
            PasswordString = "Team700!"
        });
        message.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var content = await message.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        json.GetProperty("login").GetBoolean().ShouldBe(false);
        json.GetProperty("message").GetString().ShouldBe("Invalid email or password");
    }

    [Fact]
    public async Task LogoutUser_ValidInformation_ReturnOk()
    {
        var response = await HttpClient.DeleteAsync("/api/logout");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    // [Fact]
    // public async Task LoginUser_WithPendingResetToken_CancelsResetAndSendsEmail()
    // {
    //     // add a new user
    //     await using var context = DbContextFactory.CreateDbContext();
    //     PasswordHasher<User> passwordHasher = new();
    //     var user = new User
    //     {
    //         Id = 1,
    //         Email = "test@example.com",
    //         DisplayName = "Test User",
    //         Country = "NZ",
    //         EmailVerified = true,
    //         TimeCreated = DateTime.UtcNow
    //     };
    //     user.PasswordKey = passwordHasher.HashPassword(user, "P455word!");
    //     context.Users.Add(user);
    //     await context.SaveChangesAsync();

    //     // simulate a pending reset token
    //     await HttpClient.PostAsJsonAsync("/api/user/password/reset/code/generation", new
    //     {
    //         Email = "test@example.com"
    //     });

    //     // login
    //     var response = await HttpClient.PostAsJsonAsync("/api/login", new
    //     {
    //         Email = "test@example.com",
    //         PasswordString = "Team700!"
    //     });

    //     // check the login works
    //     response.StatusCode.ShouldBe(HttpStatusCode.OK);
    //     var content = await response.Content.ReadAsStringAsync();
    //     var json = JsonSerializer.Deserialize<JsonElement>(content);
    //     json.GetProperty("login").GetBoolean().ShouldBe(true);

    //     // check email was sent, wait for the email template to be made
    //     // await ServiceProvider.GetRequiredService<IEmailService>()
    //     // .Received(1)
    //     // .SendEmailAsync(
    //     //     "test@example.com",
    //     //     EmailTemplate.ResetCancelledWarning,
    //     //     Arg.Any<Dictionary<string, string>>()
    //     // );
    // }

    [Fact]
    public async Task LoginUser_WithoutPendingResetToken_DoesNotSendWarningEmail()
    {
        // create a new user
        await using var context = DbContextFactory.CreateDbContext();
        PasswordHasher<User> passwordHasher = new();
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test User",
            Country = "NZ",
            EmailVerified = true,
            TimeCreated = DateTime.UtcNow
        };
        user.PasswordKey = passwordHasher.HashPassword(user, "Team700!");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // login
        var response = await HttpClient.PostAsJsonAsync("/api/login", new
        {
            Email = "test@example.com",
            PasswordString = "Team700!"
        });

        // check login works
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // check an email was not sent (waiting on email template)
        // await ServiceProvider.GetRequiredService<IEmailService>()
        //     .DidNotReceive()
        //     .SendEmailAsync(
        //         "test@example.com",
        //         EmailTemplate.ResetCancelledWarning,
        //         Arg.Any<Dictionary<string, string>>()
        //     );
    }
}