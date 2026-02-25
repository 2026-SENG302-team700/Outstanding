using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SENG302.Api.Services;

public interface IUserService
{
    Task CreateNewUserAsync(string email, string displayName, string passwordString, string country);
}

public class UserService : IUserService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly TimeProvider _timeProvider;

    public UserService(IDbContextFactory<DatabaseContext> dbContextFactory, TimeProvider timeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _timeProvider = timeProvider;
    }

    public async Task<User> GenerateNewUserAsync(string email, string displayName, string passwordString, string country)
    {
        PasswordHasher<User> passwordHasher = new();

        var user = new User
        {
            Email = email,
            DisplayName = displayName,
            Country = country,
            TimeCreated = _timeProvider.GetUtcNow()
        };

        var passwordKey = passwordHasher.HashPassword(user, passwordString);
        user.PasswordKey = passwordKey;

        return user;
    }

    public async Task CreateNewUserAsync(string email, string displayName, string passwordString, string country)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await GenerateNewUserAsync(email, displayName, passwordString, country);

        context.Users.Add(user);

        await context.SaveChangesAsync();
    }
}