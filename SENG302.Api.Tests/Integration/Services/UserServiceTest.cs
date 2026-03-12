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
    [InlineData("password432$#$@^PSOTH")]
    [InlineData("@#$password123SHREK")]
    [InlineData("B1er0l14m@67")]
    [InlineData("hindi-ko-alam-T4G4L0G")]
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
        User user = await ServiceUnderTest.GenerateNewUserAsync(email, displayName, "paSSWord123#@%", country);

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
        var password = "paSSWord123#@%";

        // Use the UserService function to create and add a user to the database using the information
        await ServiceUnderTest.CreateNewUserAsync(email, name, password, password, country);

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
        await ServiceUnderTest.CreateNewUserAsync("j@whitsend.com", "Jason Whitaker", "4365passTOEHKT$%^&$%^", "4365passTOEHKT$%^&$%^", "US");
        Should.Throw<DuplicateEmailException>(async () => await ServiceUnderTest.CreateNewUserAsync("j@whitsend.com", "Jack Allen", "p4ukS__45`k%NNNS", "p4ukS__45`k%NNNS", "US"));
    }
    
    [Fact]
    public async Task CreateNewUser_ShortDisplayName_InvalidDisplayNameLengthException()
    {
        await Should.ThrowAsync<InvalidDisplayNameLengthException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                "vlad@nistor.me", 
                "v", // Should throw exception
                "12345678Ab$", 
                "12345678Ab$", 
                "RO");
        });
    }

    [Fact]
    public async Task CreateNewUser_LongDisplayName_InvalidDisplayNameLengthException()
    {
        await Should.ThrowAsync<InvalidDisplayNameLengthException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                "vlad@nistor.me",
                "Vladimir Gheorghe Lucian Constantine Butnariu-Nistor-Morar-Tugurlan-ABCDEFGHIJKL", // Should throw exception
                "12345678Ab$",
                "12345678Ab$", 
                "RO"
            );
        });
    }
    [Fact]
    public async Task CreateNewUser_InvalidChars_InvalidDisplayNameCharsException()
    {
        await Should.ThrowAsync<InvalidDisplayNameCharsException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                "vlad@nistor.me",
                "Vlad Ni$tor",
                "12345678Ab$",
                "12345678Ab$", 
                "RO"
            );
        });
    }

    [Fact]
    public async Task CreateNewUser_NoEmail_InvalidEmailFormatException()
    {
        await Should.ThrowAsync<InvalidEmailFormatException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                "vlad.nistor.email",
                "Vlad Nistor",
                "12345678Ab$",
                "12345678Ab$", 
                "RO"
            );
        });
    }

    [Theory]
    [InlineData("1234")] // Short Password
    [InlineData("abcdefghi")] // All Lower Case
    [InlineData("ABCDEFGHI")] // All Upper Case
    [InlineData("123456789")] // All numeric
    [InlineData("!@#$%^&*(")] // All special char
    [InlineData("ABCdef123")] // Missing Special Char
    [InlineData("ABCdef!@#")] // Missing numeric
    [InlineData("abc123$%^")] // Missing Upper Case
    [InlineData("ABC123$%^")] // Missing Lower Case
    public async Task CreateNewUser_InvalidPassword_InvalidPasswordException(string passwordString)
    {
        await Should.ThrowAsync<InvalidPasswordException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                "vlad@nistor.email",
                "Vlad Nistor",
                passwordString,
                passwordString,
                "RO"
            );
        });
    }

    [Fact]
    public async Task CreateNewUser_MismatchedPasswords_MismatchedPasswordException()
    {
        await Should.ThrowAsync<MismatchedPasswordException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                "vlad@nistor.email",
                "Vlad Nistor",
                "ABCdef123!@#",
                "abcDEF123!@#",
                "RO");
        });
    }

    [Fact]
    public async Task CreateNewUser_BadCountry_InvalidCountryException()
    {
        await Should.ThrowAsync<InvalidCountryException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                "Vlad@nistor.email",
                "Vlad Nistor",
                "ABCdef123!@#",
                "ABCdef123!@#",
                "ROM"
            );
        });
    }
}
