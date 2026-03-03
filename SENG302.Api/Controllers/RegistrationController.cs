using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SENG302.Api;
using SENG302.Api.Filters;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using SENG302.Api.Services;


namespace SENG302.Api.Controllers;


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
    [ConditionalValidateAntiForgeryToken]
    public async Task<ActionResult<User>> RegisterUser([FromBody] PostUserRequest user)
    {
        if (string.IsNullOrWhiteSpace(user.Email) ||
        string.IsNullOrWhiteSpace(user.DisplayName) ||
        string.IsNullOrWhiteSpace(user.Country) ||
        string.IsNullOrWhiteSpace(user.PasswordKey) ||
        string.IsNullOrWhiteSpace(user.PasswordConfirm)
        ) 
        {
            return BadRequest("User registration is missing information");
        }
        
        try
        {
            await _userService.CreateNewUserAsync(user.Email, user.DisplayName, user.PasswordKey, user.PasswordConfirm, user.Country);
        }
        catch (DuplicateEmailException)
        {
            return Unauthorized("This email is already in use");
        }

        return Ok("User created successfully");
    }
}

