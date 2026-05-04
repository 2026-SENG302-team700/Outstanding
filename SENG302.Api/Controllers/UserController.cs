using System.Net;
using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SENG302.Api.Constants;
using SENG302.Api.Filters;
namespace SENG302.Api.Controllers;

[ConditionalValidateAntiForgeryToken]
[Authorize]
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFileService _fileService;
    private readonly IEmailService _emailService;
    private readonly IOneTimeCodeService _codeService;

    public UserController(IUserService userService, IFileService fileService, IOneTimeCodeService codeService, IEmailService emailService)
    {
        _userService = userService;
        _fileService = fileService;
        _codeService = codeService;
        _emailService = emailService;
    }

    /// <summary>
    /// Gets a user by their email. Returns 404 if no user with the given 
    /// email exists, or 401 if the user is not authenticated.
    /// </summary>
    /// <returns>
    /// Ok with the user details if the user is found, NotFound if the user is not found,
    /// and Unauthorized if the user email is undefined
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<User>> GetUser()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized();

        var userId = int.Parse(userIdString);
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Remove hashed password from user
        user.PasswordKey = "---";
        return Ok(user);
    }

    [AllowAnonymous]
    [HttpPost("countdown")]
    public async Task<ActionResult<long>> GetUserVerificationCountdown([FromBody] NewOneTimeCodeRequest request)
    {
        long timeElapsed;

        var user = await _userService.GetUserFromEmailAsync(request.Email);
        if (user == null) return NotFound();


        if (user.CodeGenerationTime == 0)
        {
            timeElapsed = 0;
        }
        else
        {
            timeElapsed = _codeService.GetEpochTime() - user.CodeGenerationTime;
        }

        if (timeElapsed > _codeService.TimeoutTimeSeconds)
            return Unauthorized("Code is no longer valid, account no longer exists.");

        return Ok(_codeService.TimeoutTimeSeconds - timeElapsed);
    }

    /// <summary>
    /// Updates the users information
    /// </summary>
    /// <param name="updateUserRequest"></param>
    /// <returns> The user upon successful update</returns>
    [HttpPut]
    public async Task<ActionResult<User>> UpdateUser([FromBody] UpdateUserRequest updateUserRequest)
    {
        // Get user from cookie
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized();

        // Call service to perform update logic
        try
        {
            var userId = int.Parse(userIdString);
            updateUserRequest.Email = updateUserRequest.Email.ToLower();

            var user = await _userService.UpdateUser(
                userId,
                updateUserRequest.Email,
                updateUserRequest.DisplayName,
                updateUserRequest.Country,
                updateUserRequest.ProfanityFiltering);

            if (user == null)
            {
                throw new Exception("Couldn't find user");
            }

            // Re login user to update claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.DisplayName)
            };

            var principle = new ClaimsPrincipal(
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
            );

            // Sign them in with the auth cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principle,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });
            return Ok(user);
        }
        catch (MultipleValidationException e)
        {
            return BadRequest(new BadRequestValidationResponse
            {
                Errors = e.Errors
            });
        }
        catch (Exception e)
        {
            return BadRequest(new
            {
                message = e.Message,
                errorType = e.GetType().Name
            });
        }
    }

    /// <summary>
    /// Takes an image from a Form and if valid, sends to file service for saving.
    /// Deletes the old profile picture from the file system if there is one.
    /// </summary>
    /// <param name="file">The file received from the API endpoint, should be an image</param>
    /// <param name="x">The offset of the image on the x axis</param>
    /// <param name="y">The offset of the image on the y axis</param>
    /// <param name="zoom">The amount the image is zoomed in</param>
    /// <returns>Whether the profile picture upload succeeded</returns>
    [HttpPut("pfp")]
    public async Task<ActionResult<CustomFile>> UploadProfilePicture(
        [FromForm] IFormFile file,
        [FromForm] string x,
        [FromForm] string y,
        [FromForm] string zoom)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized("You do not have authorisation to change this profile picture!");
        }

        if (file.Length > 5000000)
        {
            return BadRequest("Image too large, maximum file size is 5MB");
        }

        if (!MimeTypeSets.Images.Contains(file.ContentType))
        {
            return BadRequest("Invalid image, supported file types are .jpeg, .png, .svg, .gif .webp");
        }

        var userId = int.Parse(userIdString);
        var user = await _userService.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var userPfpId = user.ProfilePicture;

        float offsetX = float.Parse(x);
        float offsetY = float.Parse(y);
        float pfpZoom = float.Parse(zoom);

        if (userPfpId != 0)
        {
            var oldPfpFile = await _fileService.GetFileByIdAsync(userPfpId);
            await _fileService.DeleteFileAsync(oldPfpFile.FileKey);
            await _userService.SetUserProfilePicture(userId, 0);
        }

        var customFile = await _fileService.SaveFileAsync(file, userId);
        var customFileId = customFile.Id;
        await _userService.SetUserProfilePicture(userId, customFileId, offsetX, offsetY, pfpZoom);
        return Ok();
    }

    /// <summary>
    /// Gets a user's profile picture and sends it as a form
    /// </summary>
    /// <returns>A BLOB containing the user's profile picture image.</returns>
    [HttpGet("pfp")]
    public async Task<IActionResult> GetProfilePicture()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdString);
        var user = await _userService.GetUserByIdAsync(userId);

        if (user.ProfilePicture == 0)
        {
            return NotFound();
        }

        var customFile = await _fileService.GetFileByIdAsync(user.ProfilePicture);
        var fileBytes = await _fileService.GetFileContentAsync(customFile.FileKey);

        Response.Headers.Append("profile-offset-x", user.ProfilePictureOffsetX.ToString());
        Response.Headers.Append("profile-offset-y", user.ProfilePictureOffsetY.ToString());
        Response.Headers.Append("profile-offset-zoom", user.ProfilePictureZoom.ToString());

        return File(fileBytes, customFile.MimeType);
    }

    /// <summary>
    /// API Controller method that handles a put request where a one time code is requested for a specific user email.
    /// The method calls the one time code service to generate, store and then email the code to the user.
    /// </summary>
    /// <param name="codeRequest"></param> This request contains the users email which will be user to query the
    /// database and store the one time code
    /// <returns>
    /// Returns an HTTP OK 200 request if everything succeeds and a Bad Request if the email field is empty
    /// or an error occurs
    /// </returns>
    [HttpPut("password/code/generation")]
    public async Task<ActionResult<int>> InitiateOneTimeCode([FromBody] NewOneTimeCodeRequest codeRequest)
    {
        if (string.IsNullOrWhiteSpace(codeRequest.Email))
        {
            return BadRequest(new { message = "User email is missing", });
        }

        string oneTimeCode = _codeService.GenerateOneTimeCode();
        if (oneTimeCode.Length != 6) return Problem();

        User? userUpdated = await _userService.UpdateUserOneTimeCode(codeRequest.Email, oneTimeCode, 0, false);
        if (userUpdated == null) return NotFound(new { message = "No user with that email is registered" });

        // Create a dictionary of important values to send in the email, then call function to send email
        var emailDictionary = new Dictionary<string, string>
        {
            {"DISPLAY_NAME", userUpdated.DisplayName},
            {"CODE", oneTimeCode}
        };
        await _emailService.SendEmailAsync(codeRequest.Email, EmailTemplate.ChangePasswordCode, emailDictionary);

        return Ok();
    }
    /// <summary>
    /// Gets the user object from the database and compares the code the user has entered compared to the one generated
    /// to verify them. Doesnt worry about the time as this was not included in the AC.
    /// </summary>
    /// <param name="validationRequest"></param> Validation Request contain the user email which is used for querying
    /// the database and the code which the user entered on the frontend
    /// <returns>
    /// Returns an HTTP OK request if the codes match.
    /// If not, then a Bad Request is returned. If an internal server error occurs, a Problem is returned and if
    /// the User object is not found, an NotFound http error is returned. 
    /// </returns>
    [HttpPost("password/code/validation")]
    public async Task<ActionResult<bool>> ValidateOneTimeCode([FromBody] ValidateOneTimeCodeRequest validationRequest)
    {
        if (string.IsNullOrWhiteSpace(validationRequest.Email))
        {
            return BadRequest(new { message = "Invalid email", });
        }

        User? user = await _userService.GetUserFromEmailAsync(validationRequest.Email);
        if (user == null) return NotFound(new { message = "User not found" });

        bool correctCode = _codeService.CompareCodes(validationRequest.Code, user.OneTimeCode);
        if (!correctCode) return BadRequest(new { message = "Invalid Code" });

        return Ok();
    }

    /// <summary>
    /// Validates the code entered by the user on the reset password forms. Retrieves the users session tokens and compares
    /// the entered email and code to what is stored on the token, verifying it. If the session token does not exist,
    /// then it is assumed it got deleted as more than 5 minutes have past.
    /// </summary>
    /// <param name="validationRequest">A validationRequest object consisting of the users entered code and email</param>
    /// <returns>Returns an OK object result if the code and email are correct and less than 5 minutes have passed.
    /// Otherwise, if the email does not match, if the code is not correct or more than 5 minutes have passed,
    /// a Bad Request object is returned
    /// </returns>
    [AllowAnonymous]
    [HttpPost("password/reset/code/validation")]
    public async Task<ActionResult<bool>> ValidateResetPasswordCode([FromBody] ValidateOneTimeCodeRequest validationRequest)
    {
        if (string.IsNullOrWhiteSpace(validationRequest.Email))
        {
            return BadRequest(new { message = "An email is required" });
        }
        var result = await HttpContext.AuthenticateAsync("PasswordResetScheme");

        if (result.Principal == null)
        {
            await HttpContext.SignOutAsync("PasswordResetScheme");
            return BadRequest(new { message = "Code is no longer valid, please ask for a new code." });
        }
        // check email and code in body
        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        if (email != validationRequest.Email)
        {
            // return some message
            return BadRequest(new { message = "Emails do not match" });
        }

        var code = result.Principal.FindFirstValue(ClaimTypes.PostalCode);
        if (code != validationRequest.Code)
        {
            // return some message
            return BadRequest(new { message = "Incorrect code" });
        }

        if (!result.Succeeded)
        {
            // return some message
            return BadRequest(new { message = "Invalid Code or Email" });
        }

        return Ok();
    }

    /// <summary>
    /// The endpoint called for generating the code for resetting the password. The code is sent to the user
    /// via email. A new session cookie is created consisting of a 5 minute expiration timer (expires after 5 mins),
    /// code generation time, user email and code which will be used to verify the user code. 
    /// </summary>
    /// <param name="newOneTimeCodeRequest">A request object consisting of the users email</param>
    /// <returns>
    /// An OK object result if the email is valid.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("password/reset/code/generation")]
    public async Task<ActionResult<int>> GenerateResetPasswordCode(
        [FromBody] NewOneTimeCodeRequest newOneTimeCodeRequest)
    {
        long codeExpirationTime = _codeService.GetEpochTime() + 300;
        string code = _codeService.GenerateOneTimeCode();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, newOneTimeCodeRequest.Email),
            new Claim(ClaimTypes.Expiration, codeExpirationTime.ToString()),
            new Claim(ClaimTypes.PostalCode, code)
        };

        var principle = new ClaimsPrincipal(
            new ClaimsIdentity(claims, "PasswordResetScheme")
        );

        await HttpContext.SignInAsync("PasswordResetScheme", principle, new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5)
        });

        // Create a dictionary of important values to send in the email, then call function to send email
        var emailDictionary = new Dictionary<string, string>
        {
            {"MINUTES", "5"},
            {"CODE", code}
        };

        // check if email in db
        var user = await _userService.GetUserFromEmailAsync(newOneTimeCodeRequest.Email);
        if (user != null)
        {
            await _emailService.SendEmailAsync(newOneTimeCodeRequest.Email, EmailTemplate.ChangePasswordCode, emailDictionary);
        }

        // return ok no matter if there is a user or not
        return Ok();
    }


    /// <summary>
    /// Sends a request to update the users email 
    /// </summary>
    /// <param name="updatePasswordRequest"></param>
    /// <returns>response to frontend based on status of request</returns>
    [HttpPut("password/update")]
    public async Task<ActionResult> updatePassword([FromBody] UpdatePasswordRequest updatePasswordRequest)
    {
        try
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDisplayName = User.FindFirstValue(ClaimTypes.Name);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userIdString) ||
                string.IsNullOrEmpty(userDisplayName) ||
                string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { message = "Unable to find user from cookie" });
            }

            await _userService.UpdatePasswordAsync(
                int.Parse(userIdString),
                updatePasswordRequest.OldPassword,
                updatePasswordRequest.NewPassword,
                updatePasswordRequest.NewPasswordConfirm);

            // Create a dictionary of important values to send in the email, then call function to send email
            var emailDictionary = new Dictionary<string, string>
            {
                {"DISPLAY_NAME", userDisplayName}
            };
            await _emailService.SendEmailAsync(userEmail, EmailTemplate.PasswordChangedConfirmation, emailDictionary);
            return Ok();
        }
        catch (UnauthorizedAccessException e)
        {
            return Unauthorized(new { message = e.Message });
        }
        catch (MismatchedPasswordException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (InvalidPasswordException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }
    
    /// <summary>
    /// Sends a request to update the users email
    /// </summary>
    /// <param name="updatePasswordRequest"></param>
    /// <returns>response to frontend based on status of request</returns>
    [AllowAnonymous]
    [HttpPut("password/reset")]
    public async Task<ActionResult> resetPassword([FromBody] UpdatePasswordRequest updatePasswordRequest)
    {
        try
        {
            var result = await HttpContext.AuthenticateAsync("PasswordResetScheme");
            if (result.Principal == null)
            {
                await HttpContext.SignOutAsync("PasswordResetScheme");
                return BadRequest(new { message = "Code is no longer valid, please ask for a new code." });
            }
            var userEmail = result.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { message = "Unable to find user from cookie" });
            }

            await _userService.ResetPasswordAsync(
                userEmail,
                updatePasswordRequest.NewPassword,
                updatePasswordRequest.NewPasswordConfirm);
            
            // Create a dictionary of important values to send in the email, then call function to send email
            var user = await _userService.GetUserFromEmailAsync(userEmail);
            var emailDictionary = new Dictionary<string, string>
            {
                {"DISPLAY_NAME", user.Email}
            };
            await _emailService.SendEmailAsync(userEmail, EmailTemplate.PasswordChangedConfirmation, emailDictionary);
            return Ok();
        }
        catch (UnauthorizedAccessException e)
        {
            return Unauthorized(new { message = e.Message + "Unauthorized"});
        }
        catch (MismatchedPasswordException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (InvalidPasswordException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }
    
    
}