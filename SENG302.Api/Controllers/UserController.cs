using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(userEmail))
            return Unauthorized();

        var user = await _userService.GetUserByIdAsync(userEmail);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }
}