using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.ReceivedExtensions;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;

namespace SENG302.Api.Tests.Integration.Services;

public class UserServiceTest : BaseIntegrationTestFixture
{
    private IUserService ServiceUnderTest => ServiceProvider.GetRequiredService<IUserService>();

    public UserServiceTest(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }

    [Theory]
    [InlineData("password")]
    [InlineData("@#$password123shrek")]
    [InlineData("b1er0l14m")]
    [InlineData("hindi-ko-alam")]
    public async Task CreateNewUser_HashPassword_PasswordVerifies(string passwordString)
    {
        User user = await ServiceUnderTest.GenerateNewUserAsync("j@d.com", "Jedidiah Smith", passwordString, "NZ");

        var passwordHasher = new PasswordHasher<User>();

        var verify = passwordHasher.VerifyHashedPassword(user, user.PasswordKey, passwordString);

        verify.ShouldBe(PasswordVerificationResult.Success);
    }

    [Theory]
    [InlineData("jon@bler.com", "Jon Bler", "NK")]
    [InlineData("fencer@fencing.com", "FENC101", "GER")]
    [InlineData("jacob.cantebury@mail.ac.nz", "Jacob (Uni)", "AUS")]
    [InlineData("jason@jasonwhitaker.net", "Jason Whitaker", "NZ")]
    public async Task CreateNewUser_AddInformation_InformationAccurate(string email, string displayName, string country)
    {
        User user = await ServiceUnderTest.GenerateNewUserAsync(email, displayName, "password", country);

        user.Email.ShouldBe(email);
        user.DisplayName.ShouldBe(displayName);
        user.Country.ShouldBe(country);
    }
}
