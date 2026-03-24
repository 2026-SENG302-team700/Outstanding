using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;

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
    public void ValidateEmail_MalformedFormat_ExpectInvalidEmailFormatException(string email)
    {
        var service = (UserService)UserServiceUnderTest;
        using var context = DbFactory.CreateDbContext();
        
        Assert.Throws<InvalidEmailFormatException>(() => service.ValidateEmail(context, email));
    }
    
    [Theory]
    [InlineData("a")]
    [InlineData("bc")]
    [InlineData("this-name-is-definitely-longer-than-sixty-four-characters-for-testing-purposes")]
    public void ValidateDisplayName_InvalidLength_ExpectInvalidDisplayNameLengthException(string name)
    {
        var service = (UserService)UserServiceUnderTest;
        Assert.Throws<InvalidDisplayNameLengthException>(() => service.ValidateDisplayName(name));
    }
    
    [Theory]
    [InlineData("Test_L")]
    [InlineData("Test!")]
    public void ValidateDisplayName_InvalidCharacters_ExpectInvalidDisplayNameCharsException(string name)
    {
        var service = (UserService)UserServiceUnderTest;
        Assert.Throws<InvalidDisplayNameCharsException>(() => service.ValidateDisplayName(name));
    }
    
    [Fact]
    public void ValidatePassword_MismatchedPasswords_ExpectMismatchedPasswordException()
    {
        var service = (UserService)UserServiceUnderTest;
        Assert.Throws<MismatchedPasswordException>(() => 
            service.ValidatePassword("Password123!", "Password321!"));
    }
    
    [Theory]
    [InlineData("weak")]
    [InlineData("NoSpecialChars123")]
    [InlineData("nosymbolsorupper123")]
    public void ValidatePassword_WeakPassword_ExpectInvalidPasswordException(string password)
    {
        var service = (UserService)UserServiceUnderTest;
        Assert.Throws<InvalidPasswordException>(() => 
            service.ValidatePassword(password, password));
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
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        var result = await UserServiceUnderTest.CheckUserCredentialsAsync(email, "Password123!");
        Assert.Equal(UserVerificationResult.AccountUnverified, result.userVerificationResult);
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

        var updatedUser = await UserServiceUnderTest.UpdateUser(userId, "new@test.com", "New Name", "AU");

        Assert.NotNull(updatedUser);
        Assert.Equal("new@test.com", updatedUser.Email);
        Assert.Equal("New Name", updatedUser.DisplayName);
        Assert.Equal("AU", updatedUser.Country);
    }
}