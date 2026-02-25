using Microsoft.AspNetCore.Mvc;

namespace SENG302.Api.Controllers;

public class RegistrationController : ControllerBase
{
    [HttpGet (int:id)]
    public async Task<ActionResult<User>> getUser(int id)
    {
        
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<User>> CreateUser([FromBody] User user)
    {
        
    }

    [HttpPut("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] User user)
    {
        
    }
}

