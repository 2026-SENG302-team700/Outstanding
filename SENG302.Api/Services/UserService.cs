using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SENG302.Api.Services;

public interface IUserService
{
    Task<User> GenerateNewUserAsync(string email, string displayName, string passwordString, string country);
    Task CreateNewUserAsync(string email, string displayName, string passwordString, string country);
}

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException() {}

    public DuplicateEmailException(string message) : base(message) {}

    public DuplicateEmailException(string message, Exception inner) : base(message, inner) {}
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

    /// <summary>
    /// Creates a User object with the information provided and a hashed version of the password string.
    /// </summary>
    /// <param name="email">The User's email</param>
    /// <param name="displayName">The User's display name</param>
    /// <param name="passwordString">The User's password in plain text</param>
    /// <param name="country">The User's country</param>
    /// <returns>A User filled with the information provided, as well as a hashed version of the password string</returns>
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

    /// <summary>
    /// Creates and adds a User to the database based on the information provided.
    /// </summary>
    /// <param name="email">The User's email</param>
    /// <param name="displayName">The User's display name</param>
    /// <param name="passwordString">The User's password in plain text</param>
    /// <param name="country">The User's country</param>
    /// <exception cref="DuplicateEmailException">If the email is already used in the database then Duplicate Email Exception is thrown</exception>
    public async Task CreateNewUserAsync(string email, string displayName, string passwordString, string country)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        if (EmailAlreadyExists(context, email))
        {
            throw new DuplicateEmailException("This email already exists!");
        }

        var user = await GenerateNewUserAsync(email, displayName, passwordString, country);

        context.Users.Add(user);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Check if a email is already in use
    /// </summary>
    /// <param name="context">A reference to a database</param>
    /// <param name="email">We check if this email already exists in the database</param>
    /// <returns>Whether the email exists in the database</returns>
    private bool EmailAlreadyExists(DatabaseContext context, string email)
    {
        return context.Users.Where((t) => t.Email == email).Count() > 0;
    }
}