using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using SENG302.Api.Services;
using NSubstitute;
using Shouldly;
using SENG302.Api.Controllers.UserController.PasswordController;

namespace SENG302.Api.Tests.Unit.Controllers;

public class ResetPasswordUnitTests : BaseUnitTestFixture
{
    private readonly IUserService _mockUserService;
    private readonly IOneTimeCodeService _mockCodeService;
    private readonly IEmailService _mockEmailService;
    private readonly ResetPasswordController _controller;
    private readonly IAuthenticationService _mockAuthService;

    public ResetPasswordUnitTests(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory)
    {
        _mockUserService = Substitute.For<IUserService>();
        _mockCodeService = Substitute.For<IOneTimeCodeService>();
        _mockEmailService = Substitute.For<IEmailService>();
        _mockAuthService = Substitute.For<IAuthenticationService>();

        _controller = new ResetPasswordController(
            _mockUserService,
            _mockCodeService,
            _mockEmailService);
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