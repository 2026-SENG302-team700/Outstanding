using Microsoft.AspNetCore.Mvc;

namespace SENG302.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController: ControllerBase 
{
    private readonly IUserService _userService;

    public UserController(IUserService userService) 
    {
        _userService = userService;
    }

    [HttpPost("{id:string}")]
    public async Task<ActionResult<User>> CheckCredentials(string id, [FromBody] string passwordString) 
    {
        var user = await GetUserByIdAsync(id);
        if (user == null) 
        {
            return NotFound();
        } else if (user.passwordString != passwordString) {
            return Unauthorized();
        } else {
            return Ok(); //needs replacing later on
        }
    }
}