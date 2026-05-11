using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
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
        singleItemInDB.ProfanityFiltering.ShouldBe(false);

        // Make sure the password verifies properly
        var passwordHasher = new PasswordHasher<User>();
        var verify = passwordHasher.VerifyHashedPassword(singleItemInDB, singleItemInDB.PasswordKey, password);
        verify.ShouldBe(PasswordVerificationResult.Success);

        // Check that the time is accurate to the time that was given. See BaseIntegrationTestFixture.cs for TestNow
        singleItemInDB.TimeCreated.ShouldBe(TestNow);
    }

    [Fact]
    public async Task CreateNewUser_DuplicateEmail_MultipleValidationException()
    {
        await ServiceUnderTest.CreateNewUserAsync(
            "j@whitsend.com",
            "Jason Whitaker",
            "4365passTOEHKT$%^&$%^", 
            "4365passTOEHKT$%^&$%^",
            "US");
        var errors = Should.Throw<MultipleValidationException>(async () => 
            await ServiceUnderTest.CreateNewUserAsync(
                "j@whitsend.com", 
                "Jack Allen",
                "p4ukS__45`k%NNNS",
                "p4ukS__45`k%NNNS",
                "US"
                )
            );
        
        errors.Errors.Keys.ShouldContain("email");
        errors.Errors.Values.ShouldContain("This email address is already in use by another account");
    }
    
    [Fact]
    public async Task CreateNewUser_ShortDisplayName_MultipleValidationException()
    {
        var errors = await Should.ThrowAsync<MultipleValidationException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                "vlad@nistor.me", 
                "v", // Should throw exception
                "12345678Ab$", 
                "12345678Ab$", 
                "RO");
        });

        errors.Errors.Keys.ShouldContain("displayName");
        errors.Errors.Values.ShouldContain("Display name must be between 3 and 64 characters");
    }

    [Fact]
    public async Task CreateNewUser_LongDisplayName_MultipleValidationException()
    {
        var errors = await Should.ThrowAsync<MultipleValidationException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                "vlad@nistor.me",
                "Vladimir gggggggg llllll ccccccccc bbbbbbbb nnnnnn mmmm tttttttttt abcdefghijkl", // Should throw exception
                "12345678Ab$",
                "12345678Ab$", 
                "RO"
            );
        });
        
        errors.Errors.Keys.ShouldContain("displayName");
        errors.Errors.Values.ShouldContain("Display name must be between 3 and 64 characters");
    }
    [Fact]
    public async Task CreateNewUser_InvalidChars_MultipleValidationException()
    {
        var errors = await Should.ThrowAsync<MultipleValidationException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                "vlad@nistor.me",
                "Vlad Ni$tor",
                "12345678Ab$",
                "12345678Ab$",
                "RO"
            );
        });

        errors.Errors.Keys.ShouldContain("displayName");
        errors.Errors.Values.ShouldContain("Display name must only include letters, spaces, hyphens or apostrophes");
    }
    
    [Fact]
    public async Task CreateNewUser_ProfanitiesInDisplayName_MultipleValidationException()
    {
        var errors = await Should.ThrowAsync<MultipleValidationException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                "vlad@nistor.me",
                "fuck", // Should throw exception
                "12345678Ab$",
                "12345678Ab$", 
                "RO"
            );
        });
        
        errors.Errors.Keys.ShouldContain("displayName");
        errors.Errors.Values.ShouldContain("Display Name Contains Profanities! Remove Profanities!");
    }

    [Theory]
    [InlineData("vlad.nistor.email")]
    [InlineData("pandya@gmail+!.co#m")]
    [InlineData(".@gmail.com")]
    [InlineData("cool..man@gmail.com")]
    [InlineData("person@yahoo.")]
    [InlineData("crazy.@gmail.nz")]
    [InlineData("CrazyyBoyyCrazyyBoyyCrazyyBoyyCrazyyBoyyCrazyyBoyyCrazyyBoyyCrazyyBoyy@gmail.com")]
    [InlineData("man@gmailiscom.comcompanygmailiscom.comcompanygmailiscom.comcompanygmailiscom.comcompanygmailiscom.comcompanygmailiscom.comcompanygmailiscom.comcompanygmailiscom.comcompanygmailiscom.comcompanygmailiscom.comcompanygmailiscom.comcompanygmailiscom.comcompanygmailiscom.comcompany")]
    [InlineData("froggy@-outlook.com")]
    [InlineData("froggy@outlook.com-")]
    [InlineData("crazy@.nz")]
    public async Task CreateNewUser_NoEmail_MultipleValidationException(string userEmail)
    {
        var errors = await Should.ThrowAsync<MultipleValidationException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                userEmail,
                "Vlad Nistor",
                "12345678Ab$",
                "12345678Ab$", 
                "RO"
            );
        });
        
        errors.Errors.Keys.ShouldContain("email");
        errors.Errors.Values.ShouldContain("Invalid email address. Email must be in the format 'jane@doe.nz'");
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
    public async Task CreateNewUser_InvalidPassword_MultipleValidationException(string passwordString)
    {
        var errors = await Should.ThrowAsync<MultipleValidationException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                "vlad@nistor.email",
                "Vlad Nistor",
                passwordString,
                passwordString,
                "RO"
            );
        });
        
        errors.Errors.Keys.ShouldContain("password");
        errors.Errors.Values.ShouldContain("Password must be at least 8 characters long including at least one of each uppercase, lowercase, numbers and special characters");
    }

    [Fact]
    public async Task CreateNewUser_MismatchedPasswords_MultipleValidationException()
    {
        var errors = await Should.ThrowAsync<MultipleValidationException>(async () =>
        {
            await ServiceUnderTest.CreateNewUserAsync(
                "vlad@nistor.email",
                "Vlad Nistor",
                "ABCdef123!@#",
                "abcDEF123!@#",
                "RO");
        });
        
        errors.Errors.Keys.ShouldContain("passwordConfirm");
        errors.Errors.Values.ShouldContain("Passwords do not match");
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
    
    [Theory]
    [InlineData("345678", 123456789, true)]
    public async Task UpdateExistingUserCode_Success_ReturnsUpdatedUser(String code, long timeCreated, bool userVerified)
    {
        await using var context = DbContextFactory.CreateDbContext();

        string email = "testUser@gmail.com";
        
        context.Users.Add(new User
        {
            Email = email,
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        
        await context.SaveChangesAsync();
        
        // Use the TaskService function to create a new task list with the name and user email
        User? user = await ServiceUnderTest.UpdateUserOneTimeCode(email, code, timeCreated, userVerified);

        user.OneTimeCode.ShouldBe(code);
        user.CodeGenerationTime.ShouldBe(timeCreated);
        user.EmailVerified.ShouldBe(userVerified);
    }
    
    [Fact]
    public async Task DeleteUser_Success_ReturnsDeletedUser()
    {
        await using var context = DbContextFactory.CreateDbContext();

        string email = "testUser@gmail.com";
        
        context.Users.Add(new User
        {
            Email = email,
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        
        await context.SaveChangesAsync();
        
        int? id = await ServiceUnderTest.GetUserIdFromEmailAsync(email);
        // Use the TaskService function to create a new task list with the name and user email
        User? user = await ServiceUnderTest.DeleteUserByIdAsync((int)id);

        user.Id.ShouldBe((int)id);
        user.Email.ShouldBe(email);
        
        context.Users.ShouldBeEmpty();
    }

    [Fact]
    public async Task UpdateUser_ToggleProfanityFilter_ProfanityFilterToggles()
    {
        await using var context = DbContextFactory.CreateDbContext();

        var email = "hello@outstanding.com";
        var name = "Outstanding";
        var country = "NZ";
        
        context.Users.Add(new User
        {
            Email = email,
            DisplayName = name,
            PasswordKey = "B3rn$uisse",
            Country = country
        });
        
        await context.SaveChangesAsync();

        int? id = await ServiceUnderTest.GetUserIdFromEmailAsync(email);

        // First Toggle from False -> True (user should start off with profanity filtering set to off)
        User? updatedUser1 = await ServiceUnderTest.UpdateUser((int)id!, email, name, country, true);
        
        updatedUser1.ShouldNotBeNull();
        updatedUser1.ProfanityFiltering.ShouldBeTrue();
        
        // Second toggle from True -> False
        User? updateUser2 = await ServiceUnderTest.UpdateUser((int)id!, email, name, country, false);
        
        updateUser2.ProfanityFiltering.ShouldBeFalse();
    }
}
