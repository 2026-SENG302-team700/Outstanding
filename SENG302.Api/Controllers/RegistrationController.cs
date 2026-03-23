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

    public RegistrationController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> getUser(int id)
    {
        /*
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        } */
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
        user.Email = user.Email.ToLower();
        if (string.IsNullOrWhiteSpace(user.Email)) {
            return BadRequest(new
            {
                message = "Email is required!"
            });
        };
        if (string.IsNullOrWhiteSpace(user.DisplayName)) {
            return BadRequest(new
            {
                message = "Display name is required!"
            });
        }
        if (user.DisplayName.Trim().Length < 3) {
            return BadRequest(new
            {
                message = "Display name is not long enough!"
            });
        }
        if (string.IsNullOrWhiteSpace(user.Country)) {
            return BadRequest(new
            {
                message = "Country is required!"
            });
        } 
        if (string.IsNullOrWhiteSpace(user.PasswordString)) {
            return BadRequest(new
            {
                message = "A password is required!"
            });
        }
        if (string.IsNullOrWhiteSpace(user.PasswordConfirm)) {
            return BadRequest(new
            {
                message = "Passwords do not match"
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
}

