using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using SENG302.Api.Controllers;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using SENG302.Api.Services;
using NSubstitute;
using Shouldly;

namespace SENG302.Api.Tests.Unit.Controllers;

public class UserControllerUnitTests : BaseUnitTestFixture
{
    private readonly IUserService _mockUserService;
    private readonly IFileService _mockFileService;
    private readonly IOneTimeCodeService _mockCodeService;
    private readonly IEmailService _mockEmailService;
    private readonly UserController _controller;
    
    public UserControllerUnitTests(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory)
    {
        _mockUserService = Substitute.For<IUserService>();
        _mockFileService = Substitute.For<IFileService>();
        _mockCodeService = Substitute.For<IOneTimeCodeService>();
        _mockEmailService = Substitute.For<IEmailService>();

        _controller = new UserController(
            _mockUserService, 
            _mockFileService, 
            _mockCodeService, 
            _mockEmailService);
    }
    
    /// <summary>
    /// Helper method to show a logged in user by injecting the claims into the controller
    /// </summary>
    private void SetupUserContext(string userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
    
    [Fact]
    public async Task GetUser_UserExists_ReturnsOkWithNoPassword()
    {
        SetupUserContext("10");
        var mockUser = new User { Id = 10, Email = "test@test.com", DisplayName = "Test", Country = "NZ", PasswordKey = "SECRET_HASH" };
        _mockUserService.GetUserByIdAsync(10).Returns(mockUser);
        var result = await _controller.GetUser();

        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        var user = okResult.Value.ShouldBeOfType<User>();
        user.PasswordKey.ShouldBe("---");
    }
    
    [Fact]
    public async Task GetUser_UserNotFound_ReturnsNotFound()
    {
        SetupUserContext("99");
        _mockUserService.GetUserByIdAsync(99).Returns((User?)null);

        var result = await _controller.GetUser();

        result.Result.ShouldBeOfType<NotFoundResult>();
    }
    
    [Fact]
    public async Task GetUserVerificationCountdown_ValidTime_ReturnsRemainingSeconds()
    {
        var request = new NewOneTimeCodeRequest { Email = "test@test.com" };
        var mockUser = new User { Email = "test@test.com", DisplayName = "test", Country = "NZ", CodeGenerationTime = 1000 };
        
        _mockUserService.GetUserFromEmailAsync(request.Email).Returns(mockUser);
        _mockCodeService.GetEpochTime().Returns(1050);
        _mockCodeService.TimeoutTimeSeconds.Returns(300);

        var result = await _controller.GetUserVerificationCountdown(request);

        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(250L);
    }
    
    [Fact]
    public async Task GetUserVerificationCountdown_ExpiredCode_ReturnsUnauthorized()
    {
        var request = new NewOneTimeCodeRequest { Email = "test@test.com" };
        var mockUser = new User { Email = "test@test.com", DisplayName = "test", Country = "NZ", CodeGenerationTime = 1000 };
        
        _mockUserService.GetUserFromEmailAsync(request.Email).Returns(mockUser);
        _mockCodeService.GetEpochTime().Returns(2000); // past timeout
        _mockCodeService.TimeoutTimeSeconds.Returns(300);

        var result = await _controller.GetUserVerificationCountdown(request);

        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
    }
}