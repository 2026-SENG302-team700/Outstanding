using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using SENG302.Api.Services;
using SENG302.Api.Controllers;
using Shouldly;

namespace SENG302.Api.Tests.Integration.ControllerTests;

public class RegistrationControllerTest : BaseIntegrationTestFixture
{
    public RegistrationControllerTest(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

    [Fact]
    public async Task RegisterUser_SuccessfulRegistration_ReturnOk()
    {
        var data = new PostUserRequest
        {
            Email = "great.person@gmail.com",
            DisplayName = "Great Person",
            PasswordString = "Gre@tPerson69",
            PasswordConfirm = "Gre@tPerson69",
            Country = "NZ",
        };

        var message = await HttpClient.PostAsJsonAsync("/api/register", data);

        message.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Theory]
    [InlineData("shiv.sheep@gmail.com", "ShivSheep", "", "ES")]
    [InlineData("swag.mint@gmail.com", "SwagMintt", "Sheeep", "")]
    [InlineData("trad.horse@gmail.com", "", "TradTrad", "US")]
    [InlineData("", "Porcupine", "JohnPork", "US")]
    public async Task RegisterUser_MissingFields_ReturnMissingInfo(string userEmail, string userDisplayName, string passwordString, string userCountry)
    {
        var data = new
        {
            Email = userEmail,
            DisplayName = userDisplayName,
            PasswordString = passwordString,
            PasswordConfirm = passwordString,
            Country = userCountry
        };

        var message = await HttpClient.PostAsJsonAsync("/api/register", data);

        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);;
    }


    [Fact]
    public async Task RegisterUser_SameEmailTwice_ReturnMissingEmail()
    {
        var data = new
        {
            Email = "jdev@dev.com",
            DisplayName = "JJ Devy",
            PasswordString = "c00lPasSw0rdon't@ME",
            PasswordConfirm = "c00lPasSw0rdon't@ME",
            Country = "US"
        };

        var message = await HttpClient.PostAsJsonAsync("/api/register", data);

        message.StatusCode.ShouldBe(HttpStatusCode.OK);

        var data2 = new
        {
            Email = "jdev@dev.com",
            DisplayName = "JJ Devy second account",
            PasswordString = "c00lPasSw0rdon't@ME2",
            PasswordConfirm = "c00lPasSw0rdon't@ME2",
            Country = "US"
        };

        var message2 = await HttpClient.PostAsJsonAsync("/api/register", data2);
        message2.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        
        var content = await message2.Content.ReadAsStringAsync();                                                                     
        var json = JsonSerializer.Deserialize<JsonElement>(content);                                                                  
        var errors = json.GetProperty("errors");
        errors.GetProperty("email").GetString().ShouldContain("This email address is already in use by another account");
    }

    [Theory]
    [InlineData("   ")]
    public async Task GenerateCode_MissingEmail_ReturnBadRequest(string? userEmail)
    {
        var data = new
        {
            email = userEmail
        };
        var message = await HttpClient.PutAsJsonAsync("/api/register/code/generation", data);

        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        
        var content = await message.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        json.GetProperty("message").GetString().ShouldBe("User email is missing");
    }
    

    [Fact]
    public async Task ValidateCode_IncorrectCode_ReturnBadRequest()
    {
        await using var context = DbContextFactory.CreateDbContext();

        long codeGenerationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 20;

        // Add user to DB
        context.Users.Add(new User
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country",
            OneTimeCode = "608975",
            CodeGenerationTime = codeGenerationTime,
        });
        await context.SaveChangesAsync();
        var data = new
        {
            email = "test@example.com",
            Code = "609809"
        };
        
        var message = await HttpClient.PostAsJsonAsync("/api/register/code/validation", data);
        
        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        
        var content = await message.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        json.GetProperty("message").GetString().ShouldBe("Invalid Code");
    }
    
    
    [Fact]
    public async Task ValidateCode_InvalidTimedOutCode_ReturnBadRequest()
    {
        await using var context = DbContextFactory.CreateDbContext();

        long codeGenerationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 600;

        // Add user to DB
        context.Users.Add(new User
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country",
            OneTimeCode = "608975",
            CodeGenerationTime = codeGenerationTime,
        });
        await context.SaveChangesAsync();
        var data = new
        {
            email = "test@example.com",
            Code = "609809"
        };
        
        var message = await HttpClient.PostAsJsonAsync("/api/register/code/validation", data);
        
        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        
        var content = await message.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        json.GetProperty("message").GetString().ShouldBe("Code is no longer valid, account no longer exists");
    }
    
    [Fact]
    public async Task ValidateCode_ValidCode_ReturnOk()
    {
        await using var context = DbContextFactory.CreateDbContext();

        long codeGenerationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 20;

        // Add user to DB
        context.Users.Add(new User
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country",
            OneTimeCode = "608975",
            CodeGenerationTime = codeGenerationTime,
        });
        await context.SaveChangesAsync();
        var data = new
        {
            email = "test@example.com",
            Code = "608975"
        };
        
        var message = await HttpClient.PostAsJsonAsync("/api/register/code/validation", data);
        
        message.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        
    }
}