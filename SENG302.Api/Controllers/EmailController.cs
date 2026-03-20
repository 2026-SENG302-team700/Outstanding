using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Filters;

namespace SENG302.Api.Controllers;


[ConditionalValidateAntiForgeryToken]
[ApiController]
[Route("api/email")]
public class EmailController : ControllerBase {

}