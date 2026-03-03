using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using SENG302.Api.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace SENG302.Api.Controllers;

[ApiController]
[Route("api/login")]
public class LoginController : ControllerBase
{
    private readonly IUserService _userService;

    public LoginController(IUserService userService)
    {
        _userService = userService;
    }


    /// <summary>
    /// Check to ensure the provided email and password match a registered user
    /// </summary>
    /// <param name="userCredentials"> a UserCredentials object provided by the frontend containing the details used for an attempted login</param>
    /// <returns>a Task<ActionResult<User>></returns>
    [HttpPost]
    [ConditionalValidateAntiForgeryToken]
    public async Task<ActionResult<User>> CheckCredentials([FromBody] UserCredentials userCredentials)
    {
        // Ensure the credentials are correct
        var verification = await _userService.CheckUserCredentialsAsync(
            userCredentials.Email,
            userCredentials.PasswordKey
        );

        // Check if the credentials are incorrect
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
            // Create the user claims
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userCredentials.Email),
            new Claim(ClaimTypes.Email, userCredentials.Email)
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