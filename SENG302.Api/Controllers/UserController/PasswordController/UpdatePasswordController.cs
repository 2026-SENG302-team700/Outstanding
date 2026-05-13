using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SENG302.Api.Filters;
using SENG302.Api.Models.Requests.Password;

namespace SENG302.Api.Controllers.UserController.PasswordController;

[ConditionalValidateAntiForgeryToken]
[Authorize]
[ApiController]
[Route("api/user/password/update")]
public class UpdatePasswordController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly IOneTimeCodeService _codeService;

    public UpdatePasswordController(IUserService userService, IOneTimeCodeService codeService,
        IEmailService emailService)
    {
        _userService = userService;
        _codeService = codeService;
        _emailService = emailService;
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
    [HttpPut("code/generation")]
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
    [HttpPost("code/validation")]
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
    /// Sends a request to update the users email 
    /// </summary>
    /// <param name="updatePasswordRequest"></param>
    /// <returns>response to frontend based on status of request</returns>
    [HttpPut]
    public async Task<ActionResult> updatePassword([FromBody] UpdatePasswordRequest updatePasswordRequest)
    {
        try
        {
            updatePasswordRequest.OldPassword = updatePasswordRequest.OldPassword.Trim();
            updatePasswordRequest.NewPassword = updatePasswordRequest.NewPassword.Trim();
            updatePasswordRequest.NewPasswordConfirm = updatePasswordRequest.NewPasswordConfirm.Trim();
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
}