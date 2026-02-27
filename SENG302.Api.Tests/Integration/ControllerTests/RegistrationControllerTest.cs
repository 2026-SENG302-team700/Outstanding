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
        object data = new
        {
            email = "great.person@gmail.com",
            displayName = "Great Person",
            PasswordKey = "GreatPerson69",
            country = "NZ",
        };

        HttpContent myContent = JsonContent.Create(data);
        var message = await HttpClient.PostAsync("/api/register/", myContent);

        message.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Theory]
    [InlineData("shiv.sheep@gmail.com", "ShivSheep", "", "ES")]
    [InlineData("swag.mint@gmail.com", "SwagMintt", "Sheeep", "")]
    [InlineData("trad.horse@gmail.com", "", "TradTrad", "US")]
    [InlineData("", "Porcupine", "JohnPork", "US")]
    public async Task RegisterUser_MissingFields_ReturnBadRequest(String userEmail, String userDisplayName, String passwordKey, String userCountry)
    {
        object data = new
        {
            email = userEmail,
            displayName = userDisplayName,
            PasswordKey = passwordKey,
            country = userCountry,
        };

        HttpContent myContent = JsonContent.Create(data);
        var message = await HttpClient.PostAsync("/api/register/", myContent);

        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}