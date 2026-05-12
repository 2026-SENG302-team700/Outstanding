using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SENG302.Api.Helpers;

namespace SENG302.Api.Services;

public interface IUserService
{
    Task<User> GenerateNewUserAsync(string email, string displayName, string passwordString, string country);
    Task CreateNewUserAsync(string email, string displayName, string passwordString, string passwordConfirm, string country);
    Task<User?> GetUserByIdAsync(int id);
    Task<int?> GetUserIdFromEmailAsync(string email);
    Task<UserVerificationResponse> CheckUserCredentialsAsync(string email, string password);
    Task<User?> SetUserProfilePicture(int userId, int fileId, float x = 0, float y = 0, float zoom = 1);
    Task<User?> UpdateUser(int userId, string newEmail, string displayName, string country, bool profanityFiltering);
    Task<User?> UpdateUserOneTimeCode(string email, string oneTimeCode, long epochTime, bool userVerified);
    Task<User?> DeleteUserByIdAsync(int id);
    Task<User?> GetUserFromEmailAsync(string email);
    Task UpdatePasswordAsync(int userId, string oldPassword, string newPassword, string newPasswordConfirm);
    Task ResetPasswordAsync(string userEmail, string newPassword, string confirmPassword);
}

public enum UserVerificationResult
{
    DoesNotExist,
    Failed,
    MalformedEmail,
    Success,
    SuccessRehashNeeded,
    AccountUnverified
}

public class UserVerificationResponse
{
    public UserVerificationResult userVerificationResult;
    public User? user;
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
    /// Creates a new user object + hashes password
    /// </summary>
    /// <param name="email">E-mail string - e-mail of user to be generated</param> 
    /// <param name="displayName">Display Name String - Display Name of user to be generated</param>
    /// <param name="passwordString">Password String - Plaintext password of user to be hashed</param>
    /// <param name="country">Country - 2 Letter Country Code of user to be generated</param>
    /// <returns>A user object from the given information</returns>
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
    /// Validates user details then calls GenerateNewUserAsync
    /// </summary>
    /// <param name="email">E-mail string - e-mail of user to be generated</param>
    /// <param name="displayName">Display Name String - Display Name of user to be generated</param>
    /// <param name="passwordString">Password String - Plaintext password of user to be hashed</param>
    /// <param name="passwordConfirm">Password Confirm String - Plaintext confirmation of the password, should match passwordString</param>
    /// <param name="country">Country - 2 Letter Country Code of user to be generated</param>
    public async Task CreateNewUserAsync(
        string email,
        string displayName,
        string passwordString,
        string passwordConfirm,
        string country)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        
        var errors = new Dictionary<string, string>();

        foreach (var (field, message) in UserCredentialsValidator.ValidateEmail(context, email))
        {
            errors[field] = message;
        }
        foreach (var (field, message) in UserCredentialsValidator.ValidateDisplayName(displayName))
        {
            errors[field] = message;
        }
        foreach (var (field, message) in UserCredentialsValidator.ValidatePassword(passwordString, passwordConfirm))
        {
            errors[field] = message;
        }
        
        UserCredentialsValidator.ValidateCountry(country); // not added to errors as causation differs and shouldn't naturally happen.

        if (errors.Count > 0)
            throw new MultipleValidationException(errors);
        
        var user = await GenerateNewUserAsync(email, displayName, passwordString, country);

        context.Users.Add(user);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Fetch a user from the database that matches the passed in id
    /// </summary>
    /// <param name="id">a int of the provided id</param>
    /// <returns>The user that has the id that was passed in</returns>
    public async Task<User?> GetUserByIdAsync(int id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        return user;
    }

    /// <summary>
    /// Fetch a users id from the database that matches the passed in email
    /// </summary>
    /// <param name="email">a string of the provided email</param>
    /// <returns>The user id of the user that has the email that was passed in</returns>
    public async Task<int?> GetUserIdFromEmailAsync(string email)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(email.ToLower()));
        return user?.Id;
    }

    /// <summary>
    /// Fetch a user from the database that matches the passed in email
    /// </summary>
    /// <param name="email">a string of the provided email</param>
    public async Task<User?> GetUserFromEmailAsync(string email)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(email.ToLower()));

        return user;
    }

    /// <summary>
    /// Check to ensure the provided email and password match a registered user
    /// </summary>
    /// <param name="email">a string of the provided email</param>
    /// <param name="passwordString">an un-hashed string of the provided password</param>
    /// <returns>The user that matches the email and password provided or null if they do not match</returns>
    public async Task<UserVerificationResponse> CheckUserCredentialsAsync(string email, string passwordString)
    {
        PasswordHasher<User> passwordHasher = new();

        await using var context = await _dbContextFactory.CreateDbContextAsync();

        if (!UserCredentialsValidator.CheckEmailFormat(email))
        {
            return new UserVerificationResponse
            {
                userVerificationResult = UserVerificationResult.MalformedEmail,
                user = null
            };
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower().Equals(email.ToLower()));
        if (user == null)
        {
            return new UserVerificationResponse
            {
                userVerificationResult = UserVerificationResult.DoesNotExist,
                user = null
            };
        }

        if (!user.EmailVerified)
        {
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - user.CodeGenerationTime < 300)
            {
                return new UserVerificationResponse
                {
                    userVerificationResult = UserVerificationResult.AccountUnverified,
                    user = user
                };
            }
            else
            {
                await DeleteUserByIdAsync(user.Id);
                return new UserVerificationResponse
                {
                    userVerificationResult = UserVerificationResult.DoesNotExist,
                    user = user
                };
            }

        }

        PasswordVerificationResult verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordKey, passwordString);
        switch (verificationResult)
        {
            case PasswordVerificationResult.Success:
                return new UserVerificationResponse
                {
                    userVerificationResult = UserVerificationResult.Success,
                    user = user
                };

            case PasswordVerificationResult.SuccessRehashNeeded:
                return new UserVerificationResponse
                {
                    userVerificationResult = UserVerificationResult.SuccessRehashNeeded,
                    user = user
                };

            default:
                return new UserVerificationResponse
                {
                    userVerificationResult = UserVerificationResult.Failed,
                    user = null
                };
        }
    }

    /// <summary>
    /// Update the users details with the passed in values
    /// </summary>
    /// <param name="userId">a string of the provided email</param>
    /// <param name="newEmail">a string of the provided email</param>
    /// <param name="newDisplayName">a string of the users new display name</param>
    /// <param name="newCountry">a string of the users new country</param>
    /// <param name="profanityFiltering">a boolean that represents whether profanity filtering is enabled or disabled</param>
    /// <returns>The new user that has been saved in the database</returns>
    public async Task<User?> UpdateUser(int userId,
        string newEmail,
        string newDisplayName,
        string newCountry,
        bool profanityFiltering
        )
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        // Validation
        var errors = new Dictionary<string, string>();
        if (newEmail != user.Email)
        {
            foreach (var (field, message) in UserCredentialsValidator.ValidateEmail(context, newEmail))
            {
               
                errors[field] = message;
            }
        }        
        foreach (var (field, message) in UserCredentialsValidator.ValidateDisplayName(newDisplayName))
        {
            errors[field] = message;
        }
        
        UserCredentialsValidator.ValidateCountry(newCountry);

        if (errors.Count > 0)
            throw new MultipleValidationException(errors);
        
        user.Email = newEmail;
        user.DisplayName = newDisplayName;
        user.Country = newCountry;
        user.ProfanityFiltering = profanityFiltering;

        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }


    /// <summary>
    /// sets the given user's profile picture to the file id of the given image. Fails if the user does not exist
    /// Also sets the picture's offset and zoom level
    /// </summary>
    /// <param name="userId">the id of the user changing their profile picture</param>
    /// <param name="fileId">the id of the file that the user wants to add to their profile</param>
    /// <param name="x">the x offset of the image</param>
    /// <param name="y">the y offset of the image</param>
    /// <param name="zoom">the zoom level of the image</param>
    /// <returns>the user object with the new profile picture</returns>
    public async Task<User?> SetUserProfilePicture(int userId, int fileId, float x = 0, float y = 0, float zoom = 1)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        user.ProfilePicture = fileId;
        user.ProfilePictureOffsetX = x;
        user.ProfilePictureOffsetY = y;
        user.ProfilePictureZoom = zoom;

        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Updates the OneTimeCode and CodeGenerationTime attributes of the User object when the user is emailed the one time codes
    /// </summary>
    /// <param name="email"></param> The user's email
    /// <param name="oneTimeCode"></param> The code that was generated and emailed to the user or
    /// an empty string if the code is expired
    /// <param name="epochTime"></param> The time that code was generated at or 0 to represent that the code expired
    /// <param name="userVerified"></param> A boolean that notifies method whether the user has successfully verified their account or not
    /// <returns>
    /// A bool indicating if the user was updated successfully wrapped in Task object as the function is asynchronous
    /// </returns>
    public async Task<User?> UpdateUserOneTimeCode(string email, string oneTimeCode, long epochTime, bool userVerified)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        int? id = await GetUserIdFromEmailAsync(email);
        if (id == null) return null;

        User? user = await GetUserByIdAsync((int)id);
        if (user == null) return user;

        user.OneTimeCode = oneTimeCode;
        user.CodeGenerationTime = epochTime;

        if (userVerified)
        {
            user.EmailVerified = true;
        }

        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Deletes the user based on the ID
    /// </summary>
    /// <param name="id"></param> User ID
    /// <returns>A boolean representing if the user has been deleted properly</returns>
    public async Task<User?> DeleteUserByIdAsync(int id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
        return user;
    }

    /// <summary>
    /// Validates and performs the request to update the users password
    /// </summary>
    /// <param name="userId">The users id</param>
    /// <param name="oldPassword">The password that the user wishes to change from</param>
    /// <param name="newPassword">The password the user wishes to change to</param>
    /// <param name="newPasswordConfirm">the new password repeated for confirmation purpses</param>
    /// <returns>true on successful update</returns>
    public async Task UpdatePasswordAsync(int userId, string oldPassword, string newPassword, string newPasswordConfirm)
    {
        // Get user from Id
        User? user = await GetUserByIdAsync(userId);
        if  (user == null) throw new UnauthorizedAccessException("Id didn't match any user");
        
        // validate inputs
        if (!UserCredentialsValidator.ValidateUpdatePasswordRequest(user, oldPassword, newPassword, newPasswordConfirm)) return;
        
        // Perform update
        PasswordHasher<User> passwordHasher = new();
        var passwordKey = passwordHasher.HashPassword(user, newPassword);
        user.PasswordKey = passwordKey;
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Validates the inputs and throws exceptions with relevant error messages
    /// </summary>
    /// <param name="newPassword"></param>
    /// <param name="newPasswordConfirm"></param>
    /// <exception cref="MismatchedPasswordException"></exception>
    /// <exception cref="InvalidPasswordException"></exception>
    public void ValidateResetPasswordRequest(string newPassword, string newPasswordConfirm)
    {
        // Validate new passwords match
        if (!PasswordMatching(newPassword, newPasswordConfirm))
        {
            throw new MismatchedPasswordException("Passwords do not match");
        }
        
        // validate password is of valid form
        if (!CheckPassword(newPassword))
        {
            throw new InvalidPasswordException(
                "Password must be at least 8 characters long including at least one of each uppercase, lowercase, numbers and special characters"
            );
        }
    }
    
    /// <summary>
    /// Validates and performs the request to reset the users password
    /// </summary>
    /// <param name="userEmail">The users email</param>
    /// <param name="newPassword">The password the user wishes to change to</param>
    /// <param name="newPasswordConfirm">the new password repeated for confirmation purpses</param>
    /// <returns>true on successful update</returns>
    public async Task ResetPasswordAsync(string userEmail, string newPassword, string newPasswordConfirm)
    {
        // Get user from Id
        User? user = await GetUserFromEmailAsync(userEmail);
        if  (user == null) throw new UnauthorizedAccessException("email didn't match any user");
        
        // validate inputs
        ValidateResetPasswordRequest(newPassword, newPasswordConfirm);
        
        // Perform update
        PasswordHasher<User> passwordHasher = new();
        var passwordKey = passwordHasher.HashPassword(user, newPassword);
        user.PasswordKey = passwordKey;
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }
}