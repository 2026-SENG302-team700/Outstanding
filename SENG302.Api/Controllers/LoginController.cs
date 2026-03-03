using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using Microsoft.AspNetCore.Identity;
using SENG302.Api.Filters;
namespace SENG302.Api.Controllers;

[ApiController]
[Route("api/login")]
public class LoginController: ControllerBase 
{
    private readonly IUserService _userService;

    public LoginController(IUserService userService) 
    {
        _userService = userService;
    }


    /// <summary>
    /// Check to ensure the provided email and password match a registered user
    /// </summary>
    /// <param name="userCredentials">a UserCredentials object provided by the frontend containing the details used for an attempted login</param>
    /// <returns>a Task<ActionResult<User>></returns>
    [HttpPost]
    [ConditionalValidateAntiForgeryToken]
    public async Task<ActionResult<User>> CheckCredentials([FromBody] UserCredentials userCredentials) 
    {
        var verification = await _userService.CheckUserCredentialsAsync(userCredentials.Email, userCredentials.PasswordKey);
        if (verification == UserVerificationResult.DoesNotExist) 
        {
            return NotFound(new
            {
                login = false,
                message = "Invalid email or password",
                hashStatus = false
            });
        } 
        else if (verification == UserVerificationResult.MalformedEmail)
        {
            return BadRequest(new
            {
                login = false,
                message = "Invalid email address. Email must be in the format 'jane@doe.nz'",
                hashStatus = false
            });
        }
        else if (verification == UserVerificationResult.Success) 
        {
            return Ok(new
            {
                login = true,
                message = "login success",
                hashStatus = false
            });
        } 
        else if (verification == UserVerificationResult.SuccessRehashNeeded)
        {
            return Ok(new
            {
                login = true,
                message = "login success",
                hashStatus = true
            });
        }
        else 
        {
            return Unauthorized(new
            {
                login = false,
                message = "Invalid email or password",
                hashStatus = false
            }); 
        }
    }
}