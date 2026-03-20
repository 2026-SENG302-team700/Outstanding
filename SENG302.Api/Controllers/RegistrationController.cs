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

    public RegistrationController(IUserService userService, IOneTimeCodeService oneTimeCodeService)
    {
        _userService = userService;
        _oneTimeCodeService = oneTimeCodeService;
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
        if (string.IsNullOrWhiteSpace(user.Email) ||
        string.IsNullOrWhiteSpace(user.DisplayName) ||
        string.IsNullOrWhiteSpace(user.Country) ||
        string.IsNullOrWhiteSpace(user.PasswordString) ||
        string.IsNullOrWhiteSpace(user.PasswordConfirm)
        )
        {
            return BadRequest(new
            {
                message = "User registration is missing information"
            });
        }

        try
        {
            await _userService.CreateNewUserAsync(user.Email, user.DisplayName, user.PasswordString, user.PasswordConfirm, user.Country);
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
    
    [HttpGet("/code/generation")]
    public async Task<ActionResult<int>> initiateOneTimeCode([FromBody] NewOneTimeCodeRequest codeRequest)
    {
        if (string.IsNullOrWhiteSpace(codeRequest.Email))
        {
            return BadRequest(new { message = "User email is missing", });
        }
        
        int? id = await _userService.GetUserIdFromEmailAsync(codeRequest.Email);

        if (id == null)
        {
            return NotFound();
        }
        
        string oneTimeCode = _oneTimeCodeService.GenerateOneTimeCode();
        int timerStartTime = _oneTimeCodeService.GetEpochTime();

        if (oneTimeCode.Length != 6) return Problem();

        User user = await _userService.UpdateUserOneTimeCode((int)id, oneTimeCode, timerStartTime);
        
        return Ok(timerStartTime);
    }
    
}

