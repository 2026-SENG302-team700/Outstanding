using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SENG302.Api.Filters;
namespace SENG302.Api.Controllers;

[ConditionalValidateAntiForgeryToken]
[Authorize]
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Gets a user by their email. Returns 404 if no user with the given 
    /// email exists, or 401 if the user is not authenticated.
    /// </summary>
    /// <returns>
    /// Ok with the user details if the user is found, NotFound if the user is not found,
    /// and Unauthorized if the user email is undefined
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<User>> GetUser()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized();

        var userId = int.Parse(userIdString);
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Remove hashed password from user
        user.PasswordKey = "---";
        return Ok(user);
    }

    /// <summary>
    /// Updates the users information
    /// </summary>
    /// <param name="updateUserRequest"></param>
    /// <returns> The user upon successful update</returns>
    [HttpPut]
    public async Task<ActionResult<User>> UpdateUser([FromBody] UpdateUserRequest updateUserRequest)
    {
        // Get user from cookie
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Call service to perform update logic
        try
        {
            var user = await _userService.UpdateUser(int.Parse(userId), updateUserRequest.Email, updateUserRequest.DisplayName, updateUserRequest.Country);
            if (user == null)
            {
                throw new Exception("Couldn't find user");
            }

            // Re login user to update claims
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
            return Ok(user);
        }
        catch (Exception e)
        {
            return BadRequest(new
            {
                message = e.Message,
                errorType = e.GetType().Name
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<User>> UpdateUserPassword([FromBody] )
}