using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SENG302.Api.Services;

public interface IUserService
{
    Task<User> GenerateNewUserAsync(string email, string displayName, string passwordString, string country);
    Task CreateNewUserAsync(string email, string displayName, string passwordString, string country);
    Task<User?> GetUserByIdAsync(string email);
    Task<UserVerificationResult> CheckUserCredentialsAsync(string email, string password);
}

public enum UserVerificationResult 
    {
        DoesNotExist,
        Failed,
        Success,
        SuccessRehashNeeded
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

        var user = await GenerateNewUserAsync(email, displayName, passwordString, country);

        context.Users.Add(user);

        await context.SaveChangesAsync();
    }

    private bool EmailAlreadyExists(DatabaseContext context, string email)
    {
        return context.Users.Where((t) => t.Email == email).Count() > 0;
    }

    public async Task<User?> GetUserByIdAsync(string email)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Check to ensure the provided email and password match a registered user
    /// </summary>
    /// <param name="email">a string of the provided email</param>
    /// <param name="password">an un-hashed string of the provided password</param>
    /// <returns>a UserVerificationResult enum determining whether the user exists, failed, succeeded or succeeded with rehash needed verification.</returns>
    public async Task<UserVerificationResult> CheckUserCredentialsAsync(string email, string password) 
    {
        PasswordHasher<User> passwordHasher = new();
        
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) 
        {
            return UserVerificationResult.DoesNotExist;
        }
        
        PasswordVerificationResult verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordKey, password); 
        switch (verificationResult) 
        {
            case PasswordVerificationResult.Success:
                return UserVerificationResult.Success;
            
            case PasswordVerificationResult.SuccessRehashNeeded:
                return UserVerificationResult.SuccessRehashNeeded;
            
            default:
                return UserVerificationResult.Failed;
        }
    
        
        
    }
}

