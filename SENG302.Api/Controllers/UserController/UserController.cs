using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SENG302.Api.Constants;
using SENG302.Api.Filters;
using SENG302.Api.Helpers;
namespace SENG302.Api.Controllers.UserController;

[ConditionalValidateAntiForgeryToken]
[Authorize]
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFileService _fileService;
    private readonly IEmailService _emailService;
    private readonly IOneTimeCodeService _codeService;

    public UserController(IUserService userService, IFileService fileService, IOneTimeCodeService codeService, IEmailService emailService)
    {
        _userService = userService;
        _fileService = fileService;
        _codeService = codeService;
        _emailService = emailService;
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

    [AllowAnonymous]
    [HttpPost("countdown")]
    public async Task<ActionResult<long>> GetUserVerificationCountdown([FromBody] NewOneTimeCodeRequest request)
    {
        long timeElapsed;

        var user = await _userService.GetUserFromEmailAsync(request.Email);
        if (user == null) return NotFound();


        if (user.CodeGenerationTime == 0)
        {
            timeElapsed = 0;
        }
        else
        {
            timeElapsed = _codeService.GetEpochTime() - user.CodeGenerationTime;
        }

        if (timeElapsed > _codeService.TimeoutTimeSeconds)
            return Unauthorized("Code is no longer valid, account no longer exists.");

        return Ok(_codeService.TimeoutTimeSeconds - timeElapsed);
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
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized();

        // Call service to perform update logic
        try
        {
            var userId = int.Parse(userIdString);
            updateUserRequest.Email = updateUserRequest.Email.ToLower();

            var user = await _userService.UpdateUser(
                userId,
                updateUserRequest.Email,
                updateUserRequest.DisplayName,
                updateUserRequest.Country,
                updateUserRequest.ProfanityFiltering);

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
        catch (MultipleValidationException e)
        {
            return BadRequest(new BadRequestValidationResponse
            {
                Errors = e.Errors
            });
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

    /// <summary>
    /// Takes an image from a Form and if valid, sends to file service for saving.
    /// Deletes the old profile picture from the file system if there is one.
    /// </summary>
    /// <param name="file">The file received from the API endpoint, should be an image</param>
    /// <param name="x">The offset of the image on the x axis</param>
    /// <param name="y">The offset of the image on the y axis</param>
    /// <param name="zoom">The amount the image is zoomed in</param>
    /// <returns>Whether the profile picture upload succeeded</returns>
    [HttpPut("pfp")]
    public async Task<ActionResult<CustomFile>> UploadProfilePicture(
        [FromForm] IFormFile file,
        [FromForm] string x,
        [FromForm] string y,
        [FromForm] string zoom)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized("You do not have authorisation to change this profile picture!");
        }

        if (file.Length > 5000000)
        {
            return BadRequest("Image too large, maximum file size is 5MB");
        }

        var contentType = file.ContentType;
        var validContentType = MimeTypeSets.Images.Contains(contentType);
        var realContentType = ImageTypeValidation.GetRealImageMime(file);

        if (!validContentType || realContentType == ImageTypeValidation.invalidMimeString)
        {
            return BadRequest("Invalid image, supported file types are .jpeg, .png, .svg, .gif, .webp");
        }

        if (contentType != realContentType)
        {
            return BadRequest("Invalid image, file extension does not match file type");
        }

        var userId = int.Parse(userIdString);
        var user = await _userService.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var userPfpId = user.ProfilePicture;

        float offsetX = float.Parse(x);
        float offsetY = float.Parse(y);
        float pfpZoom = float.Parse(zoom);

        if (userPfpId != 0)
        {
            var oldPfpFile = await _fileService.GetFileByIdAsync(userPfpId);
            await _fileService.DeleteFileAsync(oldPfpFile.FileKey);
            await _userService.SetUserProfilePicture(userId, 0);
        }

        var customFile = await _fileService.SaveFileAsync(file, userId);
        var customFileId = customFile.Id;
        await _userService.SetUserProfilePicture(userId, customFileId, offsetX, offsetY, pfpZoom);
        return Ok();
    }

    /// <summary>
    /// Gets a user's profile picture and sends it as a form
    /// </summary>
    /// <returns>A BLOB containing the user's profile picture image.</returns>
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

        Response.Headers.Append("profile-offset-x", user.ProfilePictureOffsetX.ToString());
        Response.Headers.Append("profile-offset-y", user.ProfilePictureOffsetY.ToString());
        Response.Headers.Append("profile-offset-zoom", user.ProfilePictureZoom.ToString());

        return File(fileBytes, customFile.MimeType);
    }
    
    /// <summary>
    /// API Endpoint for toggling the profanity filter on and off.
    /// </summary>
    /// <param name="profanityFilter">A boolean with true representing profanity filtering on and false representing off</param>
    /// <returns>
    /// Unauthorized - If user is unauthorized to change the profanity filter of the requested user
    /// Ok - If profanity filter has been set.
    /// </returns>
    [Authorize]
    [HttpPatch("profanity-filter")]
    public async Task<ActionResult> ToggleProfanityFilter([FromBody] bool profanityFilter)
    {
        // get the logged in user id
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized();
        }
        
        var userId = int.Parse(userIdString);

        await _userService.ToggleProfanityFilterAsync(userId, profanityFilter);
        return Ok();
    }
}