using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using SENG302.Api.Services;
using NSubstitute;
using SENG302.Api.Controllers;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using Shouldly;

namespace SENG302.Api.Tests.Unit.Controllers;

public class RegistrationControllerUnitTests : BaseUnitTestFixture
{
    private readonly IUserService _mockUserService;
    private readonly IOneTimeCodeService _mockOneTimeCodeService;
    private readonly IEmailService _mockEmailService;
    private readonly RegistrationController _controller;

    public RegistrationControllerUnitTests(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory)
    {
        _mockUserService = Substitute.For<IUserService>();
        _mockOneTimeCodeService = Substitute.For<IOneTimeCodeService>();
        _mockEmailService = Substitute.For<IEmailService>();
        _controller = new RegistrationController(_mockUserService, _mockOneTimeCodeService, _mockEmailService);
        
    }
    
    [Theory]
    [InlineData("test@example.com")]
    public async Task GenerateCode_ValidEmail_ReturnOk(string userEmail)
    {
        long codeGenerationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 20;

        User user = new User
        {
            Email = userEmail,
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country",
            OneTimeCode = "608975",
            CodeGenerationTime = codeGenerationTime,
            EmailVerified = false,
        };
        
        var emailDictionary = new Dictionary<string, string>
        {
            {"DISPLAY_NAME", user.DisplayName},
            {"CODE", user.OneTimeCode},
            {"MINUTES", "5"}
        };
        
        _mockOneTimeCodeService.GenerateOneTimeCode().Returns(user.OneTimeCode);
        _mockOneTimeCodeService.GetEpochTime().Returns(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 20);
        _mockUserService.UpdateUserOneTimeCode(userEmail, user.OneTimeCode, codeGenerationTime, false).Returns(user);
        _mockEmailService.SendEmailAsync(user.Email, EmailTemplate.VerifyEmailCode, emailDictionary)
            .Returns(Task.CompletedTask);
            
        var data = new NewOneTimeCodeRequest
        {
            Email = userEmail,
        };
        // Call the controller directly
        var result = await _controller.initiateOneTimeCode(data);
        result.Result.ShouldBeOfType<OkResult>();
        
    }
}