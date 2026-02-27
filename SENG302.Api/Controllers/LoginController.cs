using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using Microsoft.AspNetCore.Identity;
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

    [HttpPost]
    public async Task<ActionResult<User>> CheckCredentials(UserCredentials userCredentials) 
    {
        var verification = await _userService.CheckUserCredentialsAsync(userCredentials.Email, userCredentials.PasswordKey);
        if (verification == UserVerificationResult.DoesNotExist) 
        {
            return NotFound();
        } 
        else if (verification == UserVerificationResult.Success) 
        {
            return Ok(); //needs replacing later on
        } 
        else if (verification == UserVerificationResult.SuccessRehashNeeded)
        {
            return Ok(); //will need somthing else here
        }
        else 
        {
            return Unauthorized(); 
        }
    }
}