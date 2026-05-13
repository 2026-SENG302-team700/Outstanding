using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using SENG302.Api.Helpers;
using Shouldly;

namespace SENG302.Api.Tests.Unit.Helpers;

public class UserCredentialsValidatorTests :BaseUnitTestFixture
{
    private IDbContextFactory<DatabaseContext> DbFactory => ServiceProvider.GetRequiredService<IDbContextFactory<DatabaseContext>>();
    public UserCredentialsValidatorTests(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@outstand")]
    [InlineData("@no-user.com")]
    public async Task ValidateEmail_MalformedFormat_ExpectErrorMessageInErrors(string email)
    {
        var context = await DbFactory.CreateDbContextAsync();
        
        var errors = UserCredentialsValidator.ValidateEmail(context, email);

        errors.ShouldNotBeEmpty();
        errors.ShouldHaveSingleItem();
        errors.Keys.ShouldContain("email");
        errors.Values.ShouldContain("Invalid email address. Email must be in the format 'jane@doe.nz'");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("bc")]
    [InlineData("this-name-is-definitely-longer-than-sixty-four-characters-for-testing-purposes")]
    public void ValidateDisplayName_InvalidLength_ExpectErrorMessageInErrors(string name)
    {
        var errors = UserCredentialsValidator.ValidateDisplayName(name);
        
        errors.ShouldNotBeEmpty();
        errors.ShouldHaveSingleItem();
        errors.Keys.ShouldContain("displayName");
        errors.Values.ShouldContain("Display name must be between 3 and 64 characters");
    }
    
    [Theory]
    [InlineData("Test_L")]
    [InlineData("Test!")]
    public void ValidateDisplayName_InvalidCharacters_ExpectErrorMessageInErrors(string name)
    {    
        var errors = UserCredentialsValidator.ValidateDisplayName(name);
        
        errors.ShouldNotBeEmpty();
        errors.ShouldHaveSingleItem();
        errors.Keys.ShouldContain("displayName");
        errors.Values.ShouldContain("Display name must only include letters, spaces, hyphens or apostrophes");
    }

    [Fact]
    public void ValidateDisplayName_OnlyNumbers_ExpectErrorMessageInErrors()
    { 
        var errors = UserCredentialsValidator.ValidateDisplayName("12345");
        
        errors.ShouldNotBeEmpty();
        errors.ShouldHaveSingleItem();
        errors.Keys.ShouldContain("displayName");
        errors.Values.ShouldContain("Display name must only include letters, spaces, hyphens or apostrophes");
    }
    
    [Fact]
    public void ValidatePassword_MismatchedPasswords_ExpectErrorMessageInErrors()
    {
        var errors = UserCredentialsValidator.ValidatePassword("Password123!", "Password321!");
        
        errors.ShouldNotBeEmpty();
        errors.ShouldHaveSingleItem();
        errors.Keys.ShouldContain("passwordConfirm");
        errors.Values.ShouldContain("Passwords do not match");
    }
    
    [Theory]
    [InlineData("weak")]
    [InlineData("NoSpecialChars123")]
    [InlineData("nosymbolsorupper123")]
    public void ValidatePassword_WeakPassword_ExpectErrorMessageInErrors(string password)
    {
        var errors = UserCredentialsValidator.ValidatePassword(password, password);
        
        errors.ShouldNotBeEmpty();
        errors.ShouldHaveSingleItem();
        errors.Keys.ShouldContain("password");
        errors.Values.ShouldContain("Password must be at least 8 characters long including at least one of each uppercase, lowercase, numbers and special characters"); 
    }

    [Fact]
    public void ValidateUpdatePasswordRequest_ValidData_ReturnTrue()
    {
        var user = new User
        {
            Email = "test@example.com",
            DisplayName = "Test",
            Country = "NZ",
        };
        PasswordHasher<User> passwordHasher = new();
        var passwordKey = passwordHasher.HashPassword(user, "Team700!");
        user.PasswordKey = passwordKey;

        var valid = UserCredentialsValidator.ValidateUpdatePasswordRequest(user, "Team700!", "Team701!", "Team701!");
        valid.ShouldBeTrue();
    }
    
    [Fact]
    public void ValidateUpdatePasswordRequest_CurrentPasswordNotCorrect_ThrowMismatchedPasswordException()
    {
        var user = new User
        {
            Email = "test@example.com",
            DisplayName = "Test",
            Country = "NZ",
        };
        PasswordHasher<User> passwordHasher = new();
        var passwordKey = passwordHasher.HashPassword(user, "Team700!");
        user.PasswordKey = passwordKey;

        Should.Throw<MismatchedPasswordException>(() =>
            UserCredentialsValidator.ValidateUpdatePasswordRequest(user, "no match", "Team701!", "Team701!"));
    }
    
    [Fact]
    public void ValidateUpdatePasswordRequest_NewPasswordsDontMatch_ThrowMismatchedPasswordException()
    {
        var user = new User
        {
            Email = "test@example.com",
            DisplayName = "Test",
            Country = "NZ",
        };
        PasswordHasher<User> passwordHasher = new();
        var passwordKey = passwordHasher.HashPassword(user, "Team700!");
        user.PasswordKey = passwordKey;

        Should.Throw<MismatchedPasswordException>(() =>
            UserCredentialsValidator.ValidateUpdatePasswordRequest(user, "Team700!", "Team701!", "no match"));
    }
    
    [Fact]
    public void ValidateUpdatePasswordRequest_WeakPassword_ThrowInvalidPassword()
    {
        var user = new User
        {
            Email = "test@example.com",
            DisplayName = "Test",
            Country = "NZ",
        };
        PasswordHasher<User> passwordHasher = new();
        var passwordKey = passwordHasher.HashPassword(user, "Team700!");
        user.PasswordKey = passwordKey;

        Should.Throw<InvalidPasswordException>(() =>
            UserCredentialsValidator.ValidateUpdatePasswordRequest(user, "Team700!", "weakPassword", "weakPassword"));
    }
    
    [Fact]
    public void ValidateUpdatePasswordRequest_NewPasswordSameAsOld_ThrowArgumentException()
    {
        var user = new User
        {
            Email = "test@example.com",
            DisplayName = "Test",
            Country = "NZ",
        };
        PasswordHasher<User> passwordHasher = new();
        var passwordKey = passwordHasher.HashPassword(user, "Team700!");
        user.PasswordKey = passwordKey;

        Should.Throw<ArgumentException>(() =>
            UserCredentialsValidator.ValidateUpdatePasswordRequest(user, "Team700!", "Team700!", "Team700!"));
    }
}