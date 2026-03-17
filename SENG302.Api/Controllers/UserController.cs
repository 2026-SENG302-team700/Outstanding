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
    private readonly IFileService _fileService;

    public UserController(IUserService userService, IFileService fileService)
    {
        _userService = userService;
        _fileService = fileService;
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
    
    [HttpPut]
    public async Task<ActionResult<User>> UpdateUser([FromBody] UpdateUserRequest updateUserRequest)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userService.UpdateUser(int.Parse(userId), updateUserRequest.Email, updateUserRequest.DisplayName, updateUserRequest.Country);
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

    [HttpPut("pfp")]
    public async Task<ActionResult<CustomFile>> UploadProfilePicture([FromForm] IFormFile file)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdString);
        var user = await _userService.GetUserByIdAsync(userId);
        var userPfpId = user.ProfilePicture;

        if (userPfpId != 0)
        {
            var oldPfpFile = await _fileService.GetFileByIdAsync(userPfpId);
            await _fileService.DeleteFileAsync(oldPfpFile.FileKey);
            await _userService.SetUserProfilePicture(userId, 0);
        }
        
        var customFile = await _fileService.SaveFileAsync(file, userId);
        var customFileId = customFile.Id;
        await _userService.SetUserProfilePicture(userId, customFileId);
        return Ok();
    }

    [HttpGet("pfp")]
    public async Task<IActionResult> GetProfilePicture()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdString);
        var user = await _userService.GetUserByIdAsync(userId);

        if (user.ProfilePicture == 0)
        {
            return NotFound();
        }

        var customFile = await _fileService.GetFileByIdAsync(user.ProfilePicture);
        var fileBytes = await _fileService.GetFileContentAsync(customFile.FileKey);
        
        return File(fileBytes, customFile.MimeType);
    }
}