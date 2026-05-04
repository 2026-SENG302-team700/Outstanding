using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly  IAuthenticationService _mockAuthService;
    
    public UserControllerUnitTests(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory)
    {
        _mockUserService = Substitute.For<IUserService>();
        _mockFileService = Substitute.For<IFileService>();
        _mockCodeService = Substitute.For<IOneTimeCodeService>();
        _mockEmailService = Substitute.For<IEmailService>();
        _mockAuthService = Substitute.For<IAuthenticationService>();

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
    
    
    /// <summary>
    /// Helper method to show a user who has Forgot Password by injecting the cookie claims into the controller
    /// </summary>
    private void SetupForgotPasswordContext(string email, long codeExpirationTime, string code, bool codeExpired)
    {
        
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Expiration, codeExpirationTime.ToString()),
            new Claim(ClaimTypes.PostalCode, code)
        };

        var principle = new ClaimsPrincipal(
            new ClaimsPrincipal(
                new ClaimsIdentity(claims, "TestPasswordResetScheme")
            )
        );
        
        // Code from Line 73 - 84 is attributed to Claude Code
        _mockAuthService
            .SignInAsync(
                Arg.Any<HttpContext>(),
                Arg.Any<string>(),
                Arg.Any<ClaimsPrincipal>(),
                Arg.Any<AuthenticationProperties>())
            .Returns(Task.CompletedTask);

        _mockAuthService
            .SignOutAsync(
                Arg.Any<HttpContext>(),
                Arg.Any<string>(),
                Arg.Any<AuthenticationProperties>())
            .Returns(Task.CompletedTask);

        if (codeExpired)
        {
            _mockAuthService.AuthenticateAsync(Arg.Any<HttpContext>(), "PasswordResetScheme")
                .Returns(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), "PasswordResetScheme")));
        }
        else
        {
            _mockAuthService.AuthenticateAsync(Arg.Any<HttpContext>(), "PasswordResetScheme")
                .Returns(AuthenticateResult.Success(new AuthenticationTicket(principle, "PasswordResetScheme")));
        }
        

        var serviceProvider = new ServiceCollection()
            .AddSingleton(_mockAuthService)
            .BuildServiceProvider();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principle,
                RequestServices = serviceProvider  // <-- this is what was missing
            }
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

    [Fact]
    public async Task UploadProfilePicture_ValidImage_ReplacesOldAndReturnsOk()
    {
        SetupUserContext("10");
        var mockfile = Substitute.For<IFormFile>();
        mockfile.ContentType.Returns("image/png");

        var mockUser = new User { Email = "test@test.com", Country = "NZ", DisplayName = "testName", Id = 10, ProfilePicture = 50};
        var oldFile = new CustomFile { Id = 50, OwnerId = 10, FileKey = "old-key", OriginalFileName = "old.png", MimeType = "image/png" };
        var newFile = new CustomFile { Id = 101, OwnerId = 10, FileKey = "new-key", OriginalFileName = "new.png", MimeType = "image/png" };

        _mockUserService.GetUserByIdAsync(10).Returns(mockUser);
        _mockFileService.GetFileByIdAsync(50).Returns(oldFile);
        _mockFileService.GetFileByIdAsync(101).Returns(newFile);
        _mockFileService.SaveFileAsync(mockfile, 10).Returns(newFile);

        var result = await _controller.UploadProfilePicture(mockfile, "0", "0", "0");
        result.Result.ShouldBeOfType<OkResult>();

        await _mockFileService.Received(1).DeleteFileAsync("old-key");
        await _mockUserService.Received(1).SetUserProfilePicture(10, 0, 0, 0, 1);

        await _mockUserService.Received(1).SetUserProfilePicture(10, 101, 0, 0, 0);
    }

    [Fact]
    public async Task GenerateResetPasswordCode_ValidEmail_ReturnsOk()
    {
        var request = new NewOneTimeCodeRequest { Email = "test@test.com" };
        var mockUser = new User { Email = "test@test.com", DisplayName = "test", Country = "NZ"};
        string code = "111222";
        
        _mockUserService.GetUserFromEmailAsync(request.Email).Returns(mockUser);
        _mockCodeService.GetEpochTime().Returns(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); 
        _mockCodeService.GenerateOneTimeCode().Returns(code);
        
        var emailDictionary = new Dictionary<string, string>
        {
            {"MINUTES", "5"},
            {"CODE", code}
        };
        _mockEmailService.SendEmailAsync("test@test.com", EmailTemplate.ChangePasswordCode, emailDictionary).Returns(Task.CompletedTask);
        
        SetupForgotPasswordContext(request.Email, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), code, false);
        
        var result = await _controller.GenerateResetPasswordCode(request);
        result.Result.ShouldBeOfType<OkResult>();
    }
    
    
    [Fact]
    public async Task ValidateResetPasswordCode_ValidEmailAndCode_ReturnsOk()
    {
        var request = new ValidateOneTimeCodeRequest { Email = "test@test.com", Code = "111222" };
        
        SetupForgotPasswordContext(request.Email, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), request.Code, false);
        
        var result = await _controller.ValidateResetPasswordCode(request);
        result.Result.ShouldBeOfType<OkResult>();
    }
    
    [Fact]
    public async Task ValidateResetPasswordCode_IncorrectEmail_ReturnsBadRequest()
    {
        var request = new ValidateOneTimeCodeRequest { Email = "test@test.com", Code = "111222" };
        
        SetupForgotPasswordContext("test@example.com", DateTimeOffset.UtcNow.ToUnixTimeSeconds(), request.Code, false);
        
        var result = await _controller.ValidateResetPasswordCode(request);
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
    
    [Fact]
    public async Task ValidateResetPasswordCode_IncorrectCode_ReturnsBadRequest()
    {
        var request = new ValidateOneTimeCodeRequest { Email = "test@test.com", Code = "111222" };
        
        SetupForgotPasswordContext("test@test.com", DateTimeOffset.UtcNow.ToUnixTimeSeconds(), "222222", false);
        
        var result = await _controller.ValidateResetPasswordCode(request);
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }
    
    [Fact]
    public async Task ValidateResetPasswordCode_TimedOutCode_ReturnsBadRequest()
    {
        var request = new ValidateOneTimeCodeRequest { Email = "test@test.com", Code = "111222" };
        
        SetupForgotPasswordContext("test@test.com", DateTimeOffset.UtcNow.ToUnixTimeSeconds()-400, "111222", true);
        
        var result = await _controller.ValidateResetPasswordCode(request);
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

}