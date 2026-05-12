using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using SENG302.Api.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;


namespace SENG302.Api.Controllers;

[ConditionalValidateAntiForgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[ApiController]
[Route("api")]
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
    /// <returns>a Task<ActionResult<User></returns>
    [HttpPost("login")]
    public async Task<ActionResult<User>> CheckCredentials([FromBody] UserCredentials userCredentials)
    {
        userCredentials.Email = userCredentials.Email.ToLower();
        // Ensure the credentials are correct
        var verification = await _userService.CheckUserCredentialsAsync(
            userCredentials.Email,
            userCredentials.PasswordString
        );
        var user = verification.user;
        var status = verification.userVerificationResult;

        // Check if the credentials are incorrect
        if (status == UserVerificationResult.DoesNotExist)
        {
            return NotFound(new
            {
                login = false,
                message = "Invalid email or password",
                hashStatus = false
            });
        }
        else if (status == UserVerificationResult.AccountUnverified)
        {
            return BadRequest(new
            {
                login = false,
                message = "Account is not validated yet, check your emails."
            });
        }
        else if (status == UserVerificationResult.MalformedEmail)
        {
            return BadRequest(new
            {
                login = false,
                message = "Invalid email address. Email must be in the format 'jane@doe.nz'",
                hashStatus = false
            });
        }
        else if (status == UserVerificationResult.Success && user != null)
        {
            // Create the user claims
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
            return Ok(new
            {
                login = true,
                message = user != null ? user.DisplayName : "",
                hashStatus = false
            });
        }
        else if (status == UserVerificationResult.SuccessRehashNeeded)
        {
            return Ok(new
            {
                login = true,
                message = user != null ? user.DisplayName : "",
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

    /// <summary>
    /// Removes a cookie from a browser when called upon.
    /// </summary>
    /// <returns>
    /// OK: in all cases if signoutasync fails or not (shouldn't throw exception unless something terribly goes wrong)
    /// Internal Server Error 500: if SignOutAsync throws an error (if this occurs, SignOutAsync may be deprecated)
    /// </returns>
    [Authorize]
    [HttpDelete("logout")]
    public async Task<ActionResult> LogoutUser()
    {
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}