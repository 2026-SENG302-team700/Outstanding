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

public class RegistrationControllerTest : BaseIntegrationTestFixture
{
    public RegistrationControllerTest(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }
    
    private IUserService ServiceUnderTest => ServiceProvider.GetRequiredService<IUserService>();


    [Fact]
    public async Task RegisterUser_SuccessfulRegistration_ReturnOk()
    {
        var data = new User
        {
            Email = "great.person@gmail.com",
            DisplayName = "Great Person",
            PasswordKey = "GreatPerson69",
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
    public async Task RegisterUser_MissingFields_ReturnMissingInfo(string userEmail, string userDisplayName, string passwordKey, string userCountry)
    {
        var data = new
        {
            Email = userEmail,
            DisplayName = userDisplayName,
            PasswordKey = passwordKey,
            Country = userCountry
        };

        var message = await HttpClient.PostAsJsonAsync("/api/register", data);

        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await message.Content.ReadAsStringAsync()).ShouldBe("User registration is missing information");
    }


    [Fact]
    public async Task RegisterUser_SameEmailTwice_ReturnMissingEmail()
    {
        var data = new
        {
            Email = "jdev@dev.com",
            DisplayName = "JJ Devy",
            PasswordKey = "c00lPasSw0rdon't@ME",
            Country = "AUS"
        };

        var message = await HttpClient.PostAsJsonAsync("/api/register", data);

        message.StatusCode.ShouldBe(HttpStatusCode.OK);

        var data2 = new
        {
            Email = "jdev@dev.com",
            DisplayName = "JJ Devy second account",
            PasswordKey = "c00lPasSw0rdon't@ME2",
            Country = "US"
        };

        var message2 = await HttpClient.PostAsJsonAsync("/api/register", data2);

        message2.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await message2.Content.ReadAsStringAsync()).ShouldBe("This email is already in use");
    }
}