using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;
using SENG302.Api.Controllers;
using SENG302.Api.Services;

namespace SENG302.Api.Tests.Unit.Controllers;

public class EmailControllerUnitTests : BaseUnitTestFixture {
    public EmailControllerUnitTests(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory){}
    private readonly IEmailService _mockEmailService;
    private readonly EmailController _controller;

    public EmailControllerUnitTests()
    {
        _mockEmailService = Substitute.For<IEmailService>();
    }

    [Fact]
    public async Task SendVerificationEmail_ValidData_ReturnsOk()
    {
        _mockEmailService.SendEmailAsync("test@example.com", EmailTemplate.VerifyEmailCode
    }

}