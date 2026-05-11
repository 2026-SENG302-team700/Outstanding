using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Services;
using SENG302.Api.Models.Requests;
using SENG302.Api.Models.Requests.Password;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using SENG302.Api.Filters;
namespace SENG302.Api.Controllers.UserController.PasswordController;

[ConditionalValidateAntiForgeryToken]
[Authorize]
[ApiController]
[Route("api/user/password/reset")]
public class ResetPasswordController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly IOneTimeCodeService _codeService;

    public ResetPasswordController(IUserService userService, IOneTimeCodeService codeService, IEmailService emailService)
    {
        _userService = userService;
        _codeService = codeService;
        _emailService = emailService;
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
    [HttpPost("code/generation")]
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
    [HttpPost("code/validation")]
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
    /// Sends a request to update the users email
    /// </summary>
    /// <param name="resetPasswordRequest"></param>
    /// <returns>response to frontend based on status of request</returns>
    [Authorize(AuthenticationSchemes = "PasswordResetScheme")]
    [HttpPut]
    public async Task<ActionResult> resetPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new
                {
                    message = "Code is no longer valid, please ask for a new code."
                });
            }

            await _userService.ResetPasswordAsync(
                userEmail,
                resetPasswordRequest.NewPassword,
                resetPasswordRequest.NewPasswordConfirm);
            
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