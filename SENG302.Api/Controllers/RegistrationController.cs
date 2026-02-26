using Microsoft.AspNetCore.Mvc;
using SENG302.Api;
using SENG302.Api.Models.Entities;
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

    [HttpGet ("{id:int}")]
    public async Task<ActionResult<User>> getUser(int id)
    {   
        /*
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) {
            return NotFound();
        } */
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<User>> RegisterUser([FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.Email) ||
        string.IsNullOrWhiteSpace(user.DisplayName) ||
        string.IsNullOrWhiteSpace(user.Country) ||
        string.IsNullOrWhiteSpace(user.PasswordKey)) {
            return BadRequest("User registration is missing information");
        }
        
        await _userService.CreateNewUserAsync(user.Email, user.DisplayName, user.PasswordKey, user.Country);
        
        // return CreatedAtAction(nameof(getUser), new { id = newUser.TimeCreated }, newUser);
        return Ok("User created successfully");
    }
}

