using Microsoft.AspNetCore.Mvc;

namespace SENG302.Api.Controllers;

public class RegistrationController : ControllerBase
{
    [HttpGet ("{id:int}")]
    public async Task<ActionResult<User>> getUser(int id)
    {
        return new User();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<User>> CreateUser([FromBody] User user)
    {
        return new User();
    }

    [HttpPut("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] User user)
    {
        return new User();
    }
}

