using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using Microsoft.EntityFrameworkCore;
using SENG302.Api.Controllers.UserController.PasswordController;
using SENG302.Api.Models.Requests.Password;
using SENG302.Api.Models.Requests;

namespace SENG302.Api.Tests.Integration.ControllerTests;

public class ResetPasswordTests : BaseIntegrationTestFixture
{
    private readonly IEmailService _mockEmailService;
    private readonly IOneTimeCodeService _mockOneTimeCodeService;
    private readonly ResetPasswordController _controller;

    private IUserService ServiceUnderTest => ServiceProvider.GetRequiredService<IUserService>();

    public ResetPasswordTests(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory)
    {
        _mockOneTimeCodeService = Substitute.For<IOneTimeCodeService>();
        _mockEmailService = Substitute.For<IEmailService>();
        _controller =
            new ResetPasswordController(ServiceUnderTest, _mockOneTimeCodeService, _mockEmailService);
    }

    private void SetupTempSessionContext(string email)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email)
        };

        var principle = new ClaimsPrincipal(
            new ClaimsIdentity(claims, "PasswordResetScheme")
        );

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principle }
        };
    }

    [Fact]
    public async Task ResetPassword_ValidData_PasswordReset()
    {
        var context = await DbContextFactory.CreateDbContextAsync();
        var user = new User { Email = "test@example.com", DisplayName = "Test", Country = "NZ", PasswordKey = "hash" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var userId = user.Id;
        SetupTempSessionContext("test@example.com");

        var request = new ResetPasswordRequest { NewPassword = "Test700!", NewPasswordConfirm = "Test700!" };
        var response = await _controller.resetPassword(request);
        response.ShouldBeOfType<OkResult>();

        var verifyContext = await DbContextFactory.CreateDbContextAsync();
        PasswordHasher<User> passwordHasher = new();
        var updatedUser = await verifyContext.Users.FirstOrDefaultAsync(x => x.Id == userId);

        Assert.NotNull(updatedUser);
        Assert.Equal("test@example.com", updatedUser.Email);
        Assert.Equal(PasswordVerificationResult.Success, passwordHasher.VerifyHashedPassword(updatedUser, updatedUser.PasswordKey, "Test700!"));
    }

    [Fact]
    public async Task ResetPassword_MissmatchedPasswords_BadRequest()
    {
        var context = await DbContextFactory.CreateDbContextAsync();
        var user = new User { Email = "test@example.com", DisplayName = "Test", Country = "NZ", PasswordKey = "hash" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        SetupTempSessionContext("test@example.com");

        var request = new ResetPasswordRequest { NewPassword = "Test700!", NewPasswordConfirm = "Fail700!" };
        var response = await _controller.resetPassword(request);
        response.ShouldBeOfType<BadRequestObjectResult>();
    }
}