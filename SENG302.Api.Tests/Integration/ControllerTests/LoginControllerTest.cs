using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using SENG302.Api.Controllers;
using Shouldly;

namespace SENG302.Api.Tests.Integration.ControllerTests;

public class LoginControllerTest : BaseIntegrationTestFixture
{
    public LoginControllerTest(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }


    [Theory]
    [InlineData("shiv.sheep@gmail.com", "ShivSheep", "fella", "" ,"ES")]
    [InlineData("swag.mint@gmail.com", "SwagMintt", "Sheeep", "Sheep", "NZ")]
    [InlineData("trad.horse@gmail.com", "bob", "TradTrad", "TradTrad1", "US")]
    [InlineData("steven@wilson.uk", "Porcupine", "Tree", "tree", "Uk")]
    public async Task LoginUser_IncorrectPasswords_ReturnUnauthorised(string userEmail, string userDisplayName, string passwordKey, string otherPasswordKey, string userCountry)
    {
        var registerData = new
        {
            Email = userEmail,
            DisplayName = userDisplayName,
            PasswordKey = passwordKey,
            Country = userCountry
        };
        
        var loginData = new 
        {
            Email = userEmail,
            PasswordKey = otherPasswordKey
        };
        var register = await HttpClient.PostAsJsonAsync("/api/register", registerData);
        register.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var message = await HttpClient.PostAsJsonAsync("/api/login", loginData);
        message.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await message.Content.ReadAsStringAsync()).ShouldBe("Unauthorised or otherwise failed");
    }

    [Theory]
    [InlineData("shiv.sheep@gmail.com", "ShivSheep", "fella", "shivsheep@gmail.com" ,"ES")]
    [InlineData("swag.mint@gmail.com", "SwagMintt", "Sheeep", "Sswag.mint@gmail.com", "NZ")]
    [InlineData("trad.horse@gmail.com", "bob", "TradTrad", "bob!", "US")]
    [InlineData("steven@wilson.uk", "Porcupine", "Tree", "", "Uk")]
    public async Task LoginUser_NonValidEmail_ReturnBadRequest(string userEmail, string userDisplayName, string passwordKey, string fakeEmail, string userCountry)
    {
        var registerData = new
        {
            Email = userEmail,
            DisplayName = userDisplayName,
            PasswordKey = passwordKey,
            Country = userCountry
        };
        
        var loginData = new 
        {
            Email = fakeEmail,
            // we use the same password here because these tests 
            // should fail based on an incorrect email, not password, 
            // it does not matter what password is used.
            PasswordKey = passwordKey
        };
        var register = await HttpClient.PostAsJsonAsync("/api/register", registerData);
        register.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var message = await HttpClient.PostAsJsonAsync("/api/login", loginData);
        message.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task LoginUser_CorrectLoginDetails_ReturnLoginSuccess()
    {
        var register = new
        {
            Email = "jdev@dev.com",
            DisplayName = "JJ Devy",
            PasswordKey = "c00lPasSw0rdon't@ME",
            Country = "AUS"
        };

        var message = await HttpClient.PostAsJsonAsync("/api/register", register);

        message.StatusCode.ShouldBe(HttpStatusCode.OK);

        var login = new
        {
            Email = "jdev@dev.com",
            PasswordKey = "c00lPasSw0rdon't@ME",
        };

        var message2 = await HttpClient.PostAsJsonAsync("/api/login", login);

        message2.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await message2.Content.ReadAsStringAsync()).ShouldBe("Login success");
    }
}