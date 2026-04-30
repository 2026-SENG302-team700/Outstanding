using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;

namespace SENG302.Api.Tests.Unit.Services;

public class UserServiceUnitTest : BaseUnitTestFixture
{
    private IUserService UserServiceUnderTest => ServiceProvider.GetRequiredService<IUserService>();
    private IDbContextFactory<DatabaseContext> DbFactory => ServiceProvider.GetRequiredService<IDbContextFactory<DatabaseContext>>();
    
    public UserServiceUnitTest(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@outstand")]
    [InlineData("@no-user.com")]
    public void ValidateEmail_MalformedFormat_ExpectErrorMessageInErrors(string email)
    {
        var service = (UserService)UserServiceUnderTest;
        using var context = DbFactory.CreateDbContext();
        
        var errors = service.ValidateEmail(context, email);

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
        var service = (UserService)UserServiceUnderTest;
        
        var errors = service.ValidateDisplayName(name);
        
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
        var service = (UserService)UserServiceUnderTest;
        
        var errors = service.ValidateDisplayName(name);
        
        errors.ShouldNotBeEmpty();
        errors.ShouldHaveSingleItem();
        errors.Keys.ShouldContain("displayName");
        errors.Values.ShouldContain("Display name must only include letters, spaces, hyphens or apostrophes");
    }

    [Fact]
    public void ValidateDisplayName_OnlyNumbers_ExpectErrorMessageInErrors()
    {
        var service = (UserService)UserServiceUnderTest;
        
        var errors = service.ValidateDisplayName("12345");
        
        errors.ShouldNotBeEmpty();
        errors.ShouldHaveSingleItem();
        errors.Keys.ShouldContain("displayName");
        errors.Values.ShouldContain("Display name must only include letters, spaces, hyphens or apostrophes");
    }
    
    [Fact]
    public void ValidatePassword_MismatchedPasswords_ExpectErrorMessageInErrors()
    {
        var service = (UserService)UserServiceUnderTest;

        var errors = service.ValidatePassword("Password123!", "Password321!");
        
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
        var service = (UserService)UserServiceUnderTest;
        
        var errors = service.ValidatePassword(password, password);
        
        errors.ShouldNotBeEmpty();
        errors.ShouldHaveSingleItem();
        errors.Keys.ShouldContain("password");
        errors.Values.ShouldContain("Password must be at least 8 characters long including at least one of each uppercase, lowercase, numbers and special characters"); 
    }
    
    [Fact]
    public async Task CheckUserCredentials_UserDoesNotExist_ExpectDoesNotExistResult()
    {
        var result = await UserServiceUnderTest.CheckUserCredentialsAsync("nonexistent@test.com", "AnyPassword1!");
        
        Assert.Equal(UserVerificationResult.DoesNotExist, result.userVerificationResult);
        Assert.Null(result.user);
    }
    
    [Fact]
    public async Task CheckUserCredentials_UnverifiedEmail_ExpectAccountUnverifiedResult()
    {
        var email = "unverified@test.com";
        using (var context = await DbFactory.CreateDbContextAsync())
        {
            var user = await UserServiceUnderTest.GenerateNewUserAsync(email, "Test User", "Password123!", "NZ");
            user.EmailVerified = false;
            user.CodeGenerationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 20;
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        var result = await UserServiceUnderTest.CheckUserCredentialsAsync(email, "Password123!");
        Assert.Equal(UserVerificationResult.AccountUnverified, result.userVerificationResult);
        Assert.NotNull(result.user);
    }
    
    [Fact]
    public async Task CheckUserCredentials_UnverifiedEmailTimeExpired_ExpectAccountUnverifiedResult()
    {
        var email = "unverified@test.com";
        using (var context = await DbFactory.CreateDbContextAsync())
        {
            var user = await UserServiceUnderTest.GenerateNewUserAsync(email, "Test User", "Password123!", "NZ");
            user.EmailVerified = false;
            user.CodeGenerationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 400;
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        var result = await UserServiceUnderTest.CheckUserCredentialsAsync(email, "Password123!");
        Assert.Equal(UserVerificationResult.DoesNotExist, result.userVerificationResult);
        Assert.NotNull(result.user);
    }
    
    [Fact]
    public async Task UpdateUser_ValidRequest_ExpectUpdatedUser()
    {
        int userId;
        using (var context = await DbFactory.CreateDbContextAsync())
        {
            var user = new User { Email = "old@test.com", DisplayName = "Old Name", Country = "NZ", PasswordKey = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            userId = user.Id;
        }

        var updatedUser = await UserServiceUnderTest.UpdateUser(userId, "new@test.com", "New Name", "AU", true);

        Assert.NotNull(updatedUser);
        Assert.Equal("new@test.com", updatedUser.Email);
        Assert.Equal("New Name", updatedUser.DisplayName);
        Assert.Equal("AU", updatedUser.Country);
        Assert.True(updatedUser.ProfanityFiltering);
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

        var valid = UserServiceUnderTest.ValidateUpdatePasswordRequest(user, "Team700!", "Team701!", "Team701!");
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
            UserServiceUnderTest.ValidateUpdatePasswordRequest(user, "no match", "Team701!", "Team701!"));
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
            UserServiceUnderTest.ValidateUpdatePasswordRequest(user, "Team700!", "Team701!", "no match"));
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
            UserServiceUnderTest.ValidateUpdatePasswordRequest(user, "Team700!", "weakPassword", "weakPassword"));
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
            UserServiceUnderTest.ValidateUpdatePasswordRequest(user, "Team700!", "Team700!", "Team700!"));
    }

    [Fact]
    public void ValidateProfanityFilterDisabled_NewAccount_ExpectProfanityFilterFalse()
    {
        var user = new User
        {
            Email = "hello@example.com",
            DisplayName = "Hello",
            Country = "NZ",
        };
        
        Assert.False(user.ProfanityFiltering);
    }
}