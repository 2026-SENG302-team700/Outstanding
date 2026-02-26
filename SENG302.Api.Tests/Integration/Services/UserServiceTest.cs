using System.ComponentModel.DataAnnotations;
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
    public async Task GenerateNewUser_HashPassword_PasswordVerifies(string passwordString)
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
    public async Task GenerateNewUser_AddInformation_InformationAccurate(string email, string displayName, string country)
    {
        User user = await ServiceUnderTest.GenerateNewUserAsync(email, displayName, "password", country);

        user.Email.ShouldBe(email);
        user.DisplayName.ShouldBe(displayName);
        user.Country.ShouldBe(country);
    }

    [Fact]
    public async Task CreateNewUser_ValidInformation_UserInDatabase()
    {
        // Create the variables that we are testing with
        var email = "jon@bler.com";
        var name = "Jon Bler";
        var country = "SK";
        var password = "password";

        // Use the UserService function to create and add a user to the database using the information
        await ServiceUnderTest.CreateNewUserAsync(email, name, password, country);

        await using var context = await DbContextFactory.CreateDbContextAsync();

        // Get the single item in the database
        var singleItemInDB = context.Users.ShouldHaveSingleItem();

        // Check that the information has transferred properly
        singleItemInDB.Email.ShouldBe(email);
        singleItemInDB.DisplayName.ShouldBe(name);
        singleItemInDB.Country.ShouldBe(country);

        // Make sure the password verifies properly
        var passwordHasher = new PasswordHasher<User>();
        var verify = passwordHasher.VerifyHashedPassword(singleItemInDB, singleItemInDB.PasswordKey, password);
        verify.ShouldBe(PasswordVerificationResult.Success);

        // Check that the time is accurate to the time that was given. See BaseIntegrationTestFixture.cs for TestNow
        singleItemInDB.TimeCreated.ShouldBe(TestNow);
    }

    [Fact]
    public async Task CreateNewUser_DuplicateEmail_DuplicateEmailException()
    {
        await ServiceUnderTest.CreateNewUserAsync("j@whitsend.com", "Jason Whitaker", "4365pass", "US");
        Should.Throw<DuplicateEmailException>(async () => await ServiceUnderTest.CreateNewUserAsync("j@whitsend.com", "Jack Allen", "p4ukS__45`k%", "US"));
    }
}
