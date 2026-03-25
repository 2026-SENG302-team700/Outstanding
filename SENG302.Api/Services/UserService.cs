using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using SENG302.Api.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SENG302.Api.Services;

public interface IUserService
{
    bool ValidateUpdatePasswordRequest(User user, string oldPassword, string newPassword, string newPasswordConfirm);
    Task<User> GenerateNewUserAsync(string email, string displayName, string passwordString, string country);
    Task CreateNewUserAsync(string email, string displayName, string passwordString, string passwordConfirm, string country);
    Task<User?> GetUserByIdAsync(int id);
    Task<int?> GetUserIdFromEmailAsync(string email);
    Task<UserVerificationResponse> CheckUserCredentialsAsync(string email, string password);
    Task<User?> UpdateUser(int userId, string newEmail, string displayName, string country);
    Task<User?> UpdateUserOneTimeCode(string email, string oneTimeCode, long epochTime, bool userVerified);
    Task<User?> DeleteUserByIdAsync(int id);
    Task<User?> SetUserProfilePicture(int userId, int fileId);
    Task<User?> GetUserFromEmailAsync(string email);
    Task<bool> UpdatePasswordAsync(int userId, string oldEmail, string newEmail, string newEmailConfirm);
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


public class InvalidCodeException : Exception
{
    public InvalidCodeException() { }

    public InvalidCodeException(string message) : base(message) { }

    public InvalidCodeException(string message, Exception inner) : base(message, inner) { }
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
    /// Runs all email validation checks and throws relavent exceptions
    /// </summary>
    /// <param name="context"></param>
    /// <param name="email"></param>
    /// <exception cref="DuplicateEmailException"></exception>
    /// <exception cref="InvalidEmailFormatException"></exception>
    public void ValidateEmail(DatabaseContext context, string email)
    {
        if (EmailAlreadyExists(context, email))
        {
            throw new DuplicateEmailException("This email address is already in use by another account");
        }

        if (!CheckEmailFormat(email))
        {
            throw new InvalidEmailFormatException("Invalid email address. Email must be in the format ‘jane@doe.nz’");
        }
    }

    /// <summary>
    /// Runs all display name validations and throws relavent exceptions
    /// </summary>
    /// <param name="displayName"></param>
    /// <exception cref="InvalidDisplayNameLengthException"></exception>
    /// <exception cref="InvalidDisplayNameCharsException"></exception>
    public void ValidateDisplayName(string displayName)
    {
        if (DisplayNameLength(displayName))
        {
            throw new InvalidDisplayNameLengthException("Display name must be between 3 and 64 characters");
        }

        if (!DisplayNameChars(displayName))
        {
            throw new InvalidDisplayNameCharsException(
                "Display name must only include letters, spaces, hyphens or apostrophes"
                );
        }
    }
    /// <summary>
    /// Runs all password validations and throws relavent excpetions
    /// </summary>
    /// <param name="passwordOne"></param>
    /// <param name="passwordTwo"></param>
    /// <exception cref="MismatchedPasswordException"></exception>
    /// <exception cref="InvalidPasswordException"></exception>
    public void ValidatePassword(string passwordOne, string passwordTwo)
    {
        if (!PasswordMatching(passwordOne, passwordTwo))
        {
            throw new MismatchedPasswordException("Passwords do not match");
        }

        if (!CheckPassword(passwordOne))
        {
            throw new InvalidPasswordException(
                "Password must be at least 8 characters long including at least one of each uppercase, lowercase, numbers and special characters"
            );
        }
    }

    /// <summary>
    /// Runs all country validations and throws relavent exceptions
    /// </summary>
    /// <param name="country"></param>
    /// <exception cref="InvalidCountryException"></exception>
    public void ValidateCountry(string country)
    {
        if (!ValidCountry(country))
        {
            throw new InvalidCountryException(
                "Invalid Country ISO code -- Front End sending wrong country codes"
            );
        }
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

        ValidateEmail(context, email);
        ValidateDisplayName(displayName);
        ValidatePassword(passwordString, passwordConfirm);
        ValidateCountry(country);

        var user = await GenerateNewUserAsync(email, displayName, passwordString, country);

        context.Users.Add(user);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Checks to see if display name length is between 3-64 characters
    /// </summary>
    /// <param name="displayName">String representing display name of the user</param>
    /// <returns>
    /// True: If display name is between 3-64 characters
    /// False: If display name is less than 3 characters or greater than 64 characters
    /// </returns>
    private bool DisplayNameLength(string displayName)
    {
        return (displayName.Length < 3) || (displayName.Length > 64);
    }

    /// <summary>
    /// Checks to see if the display name characters are valid
    /// </summary>
    /// <param name="displayName">String representing display name of the user</param>
    /// <returns>
    /// True: If display name only contains allowed characters
    /// False: If display name contains any disallowed characters
    /// </returns>
    private bool DisplayNameChars(string displayName)
    {
        return ValidationPatterns.UserDisplayName.IsMatch(displayName);
    }

    /// <summary>
    /// Checks to see if the e-mail string is in a valid format
    /// </summary>
    /// <param name="email"> String representing e-mail of the user</param>
    /// <returns>
    /// True: E-mail is a valid format
    /// False: E-mail is an invalid format
    /// </returns>
    private bool CheckEmailFormat(string email)
    {
        // Gotten from C# docs
        try
        {
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.

            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                string domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }

        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        try
        {
            return ValidationPatterns.UserEmail.IsMatch(email);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks to see if password meets strength requirements
    /// </summary>
    /// <param name="password">Plaintext string representation of the users password</param>
    /// <returns>
    /// True: Password meets password strength requirements
    /// False: Password does not meet password strength requirements
    /// </returns>
    private bool CheckPassword(string password)
    {
        return ValidationPatterns.UserPassword.IsMatch(password);
    }

    /// <summary>
    /// Checks to see if the two provided passwords are matching
    /// </summary>
    /// <param name="passwordString">Plaintext string representation of the users password</param>
    /// <param name="passwordConfirmation">Plaintext string representation of the password confirmation</param>
    /// <returns>
    /// True: passwords match
    /// False: passwords differ
    /// </returns>
    private bool PasswordMatching(string passwordString, string passwordConfirmation)
    {
        return passwordString == passwordConfirmation;
    }

    /// <summary>
    /// Checks to see if the provided country code is valid or not
    /// </summary>
    /// <param name="country">string representation of a 2-letter country code</param>
    /// <returns>
    /// True: country is in the country code constants list
    /// False: country is not in the country code constants list
    /// </returns>
    private bool ValidCountry(string country)
    {
        return CountryCodes.All.Contains(country);
    }

    /// <summary>
    /// Checks to see if the provided e-mail is already in the database
    /// </summary>
    /// <param name="context">The Database</param>
    /// <param name="email">String representation of the e-mail</param>
    /// <returns>
    /// True: e-mail is already stored and associated with an account in the db
    /// False: e-mail is not stored and associated with an account in the db
    /// </returns>
    private bool EmailAlreadyExists(DatabaseContext context, string email)
    {
        return context.Users.Where((t) => t.Email == email).Count() > 0;
    }

    /// <summary>
    /// Fetch a user from the database that matches the passed in id
    /// </summary>
    /// <param name="id">a int of the provided id</param>
    /// <returns>The user that has the id that was passed in</returns>
    public async Task<User?> GetUserByIdAsync(int id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        return await context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// Fetch a users id from the database that matches the passed in email
    /// </summary>
    /// <param name="email">a string of the provided email</param>
    /// <returns>The user id of the user that has the email that was passed in</returns>
    public async Task<int?> GetUserIdFromEmailAsync(string email)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user?.Id;
    }

    /// <summary>
    /// Fetch a user from the database that matches the passed in email
    /// </summary>
    /// <param name="email">a string of the provided email</param>
    public async Task<User?> GetUserFromEmailAsync(string email)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
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

        if (!CheckEmailFormat(email))
        {
            return new UserVerificationResponse
            {
                userVerificationResult = UserVerificationResult.MalformedEmail,
                user = null
            };
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
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
            return new UserVerificationResponse
            {
                userVerificationResult = UserVerificationResult.AccountUnverified,
                user = user
            };
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
    /// <returns>The new user that has been saved in the database</returns>
    public async Task<User?> UpdateUser(int userId,
        string newEmail,
        string newDisplayName,
        string newCountry
        )
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        // Validation
        if (user.Email != newEmail) ValidateEmail(context, newEmail);
        if (user.DisplayName != newDisplayName) ValidateDisplayName(newDisplayName);
        if (user.Country != newCountry) ValidateCountry(newCountry);

        user.Email = newEmail;
        user.DisplayName = newDisplayName;
        user.Country = newCountry;

        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> SetUserProfilePicture(int userId, int fileId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        user.ProfilePicture = fileId;

        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Updates the OneTimeCode and CodeGenerationTime attributes of the User object when the user is emailed the one time codes
    /// </summary>
    /// <param name="user"></param> The user object
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
    /// Checks the password hash and returns a verification result
    /// </summary>
    /// <param name="user">The user you are checking the password for</param>
    /// <param name="password">the password you are checking matches the user</param>
    /// <returns>The verification result</returns>
    public PasswordVerificationResult VerifyPassword(User user, string password)
    {
        PasswordHasher<User> passwordHasher = new();
        return passwordHasher.VerifyHashedPassword(user, user.PasswordKey, password);
    }
    
    /// <summary>
    /// Performs all validation for the update password request
    /// </summary>
    /// <param name="user">The users whose password is being updated</param>
    /// <param name="oldPassword">The password that the user wishes to change from</param>
    /// <param name="newPassword">The password the user wishes to change to</param>
    /// <param name="newPasswordConfirm">the new password repeated for confirmation purpses</param>
    /// <returns></returns>
    /// <exception cref="MismatchedPasswordException"></exception>
    /// <exception cref="InvalidPasswordException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public bool ValidateUpdatePasswordRequest(User user, string oldPassword, string newPassword,
        string newPasswordConfirm)
    {
        // Validate old password is the users correct password
        var result = VerifyPassword(user, oldPassword);
        if (!(result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded))
        {
            throw new MismatchedPasswordException("Current password was incorrect");
        }
        
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
        
        // Validate new password not the same as old password
        if (PasswordMatching(newPassword, oldPassword))
        {
            throw new ArgumentException("New password can't be the same as old password");
        }

        return true;
    }

    /// <summary>
    /// Validates and performs the request to update the users password
    /// </summary>
    /// <param name="userId">The users id</param>
    /// <param name="oldPassword">The password that the user wishes to change from</param>
    /// <param name="newPassword">The password the user wishes to change to</param>
    /// <param name="newPasswordConfirm">the new password repeated for confirmation purpses</param>
    /// <returns>true on successful update</returns>
    public async Task<bool> UpdatePasswordAsync(int userId, string oldPassword, string newPassword, string newPasswordConfirm)
    {
        // Get user from Id
        User? user = await GetUserByIdAsync(userId);
        if  (user == null) throw new UnauthorizedAccessException("Id didn't match any user");
        
        // validate inputs
        if (!ValidateUpdatePasswordRequest(user, oldPassword, newPassword, newPasswordConfirm)) return false;
        
        // Perform update
        PasswordHasher<User> passwordHasher = new();
        var passwordKey = passwordHasher.HashPassword(user, newPassword);
        user.PasswordKey = passwordKey;
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return true;
    }
}