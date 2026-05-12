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