using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using Microsoft.AspNetCore.Identity;
using SENG302.Api.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


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
        var user = await _userService.ValidateCredentialsAsync(
            userCredentials.Email,
            userCredentials.PasswordKey
        );

        if (user == null)
        {
            return Unauthorized("Invalid credentials");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var principle = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
        );

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principle,
            new AuthenticationProperties{
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });
        
        return Ok(new LoginResponse{
            Email = user.Email,
            DisplayName = user.DisplayName
        });
    }

    public class LoginResponse
    {
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}