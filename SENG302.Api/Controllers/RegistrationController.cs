using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;

namespace SENG302.Api.Controllers;


[ApiController]
public class RegistrationController : ControllerBase
{
    private readonly IUserService _userService;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> getUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<User>> RegisterUser([FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.Email) ||
        string.IsNullOrWhiteSpace(user.DisplayName) ||
        string.IsNullOrWhiteSpace(user.Country) ||
        string.IsNullOrWhiteSpace(user.PasswordKey))
        {
            return BadRequest("User registration is missing information");
        }

        var newUser = await _userService.CreateNewUserAsync(user.Email, user.DisplayName, user.PasswordKey, user.Country);
        return CreatedAtAction(nameof(getUser), new { id = newUser.TimeCreated }, newUser);
    }


    [HttpPut("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] User user)
    {
        return new User();
    }
}

