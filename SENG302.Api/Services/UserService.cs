using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Text.RegularExpressions;
using System.Net.Mail;

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

public class InvalidDisplayNameLengthException : Exception
{
    public InvalidDisplayNameLengthException() {}
    
    public InvalidDisplayNameLengthException(string message) : base(message) {}
    
    public InvalidDisplayNameLengthException(string message, Exception inner) : base(message, inner) {}
}

public class InvalidDisplayNameCharsException : Exception
{
    public InvalidDisplayNameCharsException() {}
    
    public InvalidDisplayNameCharsException(string message) : base(message) {} 
    
    public InvalidDisplayNameCharsException(string message, Exception inner) : base(message, inner) {}
}

public class InvalidEmailFormatException : Exception
{
    public InvalidEmailFormatException() {}
    
    public InvalidEmailFormatException(string message) : base(message) {}
    
    public InvalidEmailFormatException(string message, Exception inner) : base(message, inner) {}
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
        
        if (EmailAlreadyExists(context, email))
        {
            throw new DuplicateEmailException("This email already exists!");
        }

        if (DisplayNameLength(displayName))
        {
            throw new InvalidDisplayNameLengthException("Display name must be between 3 and 64 characters");
        }

        if (DisplayNameChars(displayName))
        {
            throw new InvalidDisplayNameCharsException("Display name must only include letters, spaces, hyphens or apostrophes");
        }

        if (!CheckEmailFormat(email))
        {
            throw new InvalidEmailFormatException("Invalid email address. Email must be in the format ‘jane@doe.nz’");
        }

        var user = await GenerateNewUserAsync(email, displayName, passwordString, country);

        context.Users.Add(user);

        await context.SaveChangesAsync();
    }

    private bool DisplayNameLength(string displayName)
    {
        return ((displayName.Length < 3) || (displayName.Length > 64));
    }

    private bool DisplayNameChars(string displayName)
    {
        // regex below allow a-z, A-Z, - and ' -- 
        var validCharsRegex = new Regex(
            "^[a-zA-Z\\-']+$",
            RegexOptions.None, // Regex Options, can ignore, 
            TimeSpan.FromSeconds(2) // TimeSpan until regex times out
            ); 
        return (validCharsRegex.IsMatch(displayName));
    }

    private bool CheckEmailFormat(string email)
    {
        try
        {
            MailAddress m = new MailAddress(email); // throws exception if not in form of e-mail
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    
    private bool EmailAlreadyExists(DatabaseContext context, string email)
    {
        return context.Users.Where((t) => t.Email == email).Count() > 0;
    }
}