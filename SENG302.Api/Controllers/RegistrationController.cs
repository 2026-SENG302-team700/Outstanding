using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Filters;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using SENG302.Api.Services;


namespace SENG302.Api.Controllers;

[ConditionalValidateAntiForgeryToken]
[ApiController]
[Route("api/register")]
public class RegistrationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IOneTimeCodeService _oneTimeCodeService;
    private readonly IEmailService _emailService;

    public RegistrationController(IUserService userService, IOneTimeCodeService oneTimeCodeService, IEmailService emailService)
    {
        _userService = userService;
        _oneTimeCodeService = oneTimeCodeService;
        _emailService = emailService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok();
    }

    /// <summary>
    /// API Controller method that handles a post request for registering 
    /// a new user and hence creating a new user object.
    /// </summary>
    /// <param name="user">User is a user object from frontend</param>
    /// <returns>
    /// Returns the Http OK response for successful register of a User and a
    /// bad request if any fields are empty
    /// </returns>
    [HttpPost]
    public async Task<ActionResult<User>> RegisterUser([FromBody] PostUserRequest user)
    {
        var errors = new Dictionary<string, string>();
        
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            errors["email"] = "Invalid email address. Email must be in the format 'jane@doe.nz'";
        }
        
        var displayName = user.DisplayName.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            errors["displayName"] = "Display name must be between 3 and 64 characters";
        }
        
        if (string.IsNullOrWhiteSpace(user.Country))
        {
            return BadRequest(new
            {
                message = "Country is required!"
            });
        }
        
        if (string.IsNullOrWhiteSpace(user.PasswordString))
        {
            errors["password"] =
                "Password must be at least 8 characters long including at least one of each uppercase, lowercase, numbers and special characters";
        }
        
        if (string.IsNullOrWhiteSpace(user.PasswordConfirm))
        {
            errors["passwordConfirm"] = "Passwords do not match";
        }

        if (errors.Count > 0)
        {
            return BadRequest(new BadRequestValidationResponse
            {
                Errors = errors
            });
        }
        
        try
        {
            await _userService.CreateNewUserAsync(user.Email, user.DisplayName, user.PasswordString, user.PasswordConfirm, user.Country);
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

        return Ok(new
        {
            message = "Registration successful. Please log in."
        });
    }

    /// <summary>
    /// API Controller method that handles a put request where a one time code and the epoch time (seconds since 1st Jan 1970) is
    /// generated and stored in the User 
    /// </summary>
    /// <param name="codeRequest"></param> This request contains the users email which will be user to query the
    /// database and store the one time code along with the time the code was generated at.
    /// <returns>
    /// Returns an HTTP OK 200 request if everything succeeds and a Bad Request if the email field is empty
    /// or an error occurs
    /// </returns>
    [HttpPut("code/generation")]
    public async Task<ActionResult<int>> initiateOneTimeCode([FromBody] NewOneTimeCodeRequest codeRequest)
    {
        Console.WriteLine("\n\n Does it reach backend \n\n");
        if (string.IsNullOrWhiteSpace(codeRequest.Email))
        {
            return BadRequest(new { message = "User email is missing", });
        }

        string oneTimeCode = _oneTimeCodeService.GenerateOneTimeCode();
        long timerStartTime = _oneTimeCodeService.GetEpochTime();

        User? userUpdated;

        if (codeRequest.ResendingCode)
        {
            User? user = await _userService.GetUserFromEmailAsync(codeRequest.Email);
            if (user == null) return Problem();
            userUpdated = await _userService.UpdateUserOneTimeCode(codeRequest.Email, oneTimeCode, user.CodeGenerationTime, false);
        }
        else
        {
            userUpdated = await _userService.UpdateUserOneTimeCode(codeRequest.Email, oneTimeCode, timerStartTime, false);
        }

        if (userUpdated == null) return Problem();

        // Create a dictionary of important values to send in the email, then call function to send email
        var emailDictionary = new Dictionary<string, string>
        {
            {"DISPLAY_NAME", userUpdated.DisplayName},
            {"CODE", oneTimeCode},
            {"MINUTES", "5"}
        };
        await _emailService.SendEmailAsync(userUpdated.Email, EmailTemplate.VerifyEmailCode, emailDictionary);

        return Ok();
    }

    /// <summary>
    /// Gets the user object from the database and compares the code the user has entered compared to the one generated
    /// to verify them. Also compares the time created and the time currently to see if it is under the time limit.
    /// Deletes the user object is the time limit is over. O
    /// </summary>
    /// <param name="validationRequest"></param> Validation Request contain the user email which is used for querying
    /// the database and the code which the user entered on the frontend
    /// <returns>
    /// Returns an HTTP OK request if the codes match and the time since code generation is under the time limit.
    /// If not, then a Bad Request is returned. If an internal server error occurs, a Problem is returned and if
    /// the User object is not found, an NotFound http error is returned. 
    /// </returns>
    [HttpPost("code/validation")]
    public async Task<ActionResult<bool>> validateOneTimeCode([FromBody] ValidateOneTimeCodeRequest validationRequest)
    {
        long codeEnteredTime = _oneTimeCodeService.GetEpochTime();

        if (string.IsNullOrWhiteSpace(validationRequest.Email))
        {
            return BadRequest(new { message = "Invalid email", });
        }

        User? user = await _userService.GetUserFromEmailAsync(validationRequest.Email);
        if (user == null) return NotFound(new { message = "User not found" });

        CodeVerificationResult codeVerificationResult =
            _oneTimeCodeService.VerfiyCode(user, codeEnteredTime, validationRequest.Code);


        if (codeVerificationResult == CodeVerificationResult.CodeExpired)
        {
            await _userService.DeleteUserByIdAsync(user.Id);
            return BadRequest(new { message = "Code is no longer valid, account no longer exists" });
        }

        if (codeVerificationResult == CodeVerificationResult.CodeIncorrect)
        {
            return BadRequest(new { message = "Invalid Code" });
        }

        if (codeVerificationResult == CodeVerificationResult.CodeSuccessful)
        {
            User? userUpdated = await _userService.UpdateUserOneTimeCode(user.Email, "", 0, true);
            if (userUpdated == null) return Problem();
        }

        return Ok();
    }

}




