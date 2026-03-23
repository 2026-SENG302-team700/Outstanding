using Microsoft.Extensions.Options;
using Xunit;
using NSubstitute;
using SENG302.Api.Services;
using MimeKit;

namespace SENG302.Api.Tests.Unit.Services;

public class EmailServiceUnitTests
{
    private readonly ISmtpClientWrapper _smtpMock = Substitute.For<ISmtpClientWrapper>();
    private readonly EmailService _service;

    public EmailServiceUnitTests()
    {
        var settings = Options.Create(new EmailSettings {
            FromEmail = "test@example.com",
            Host = "smtp.test.com"
        });
        // inject the mock into the real service
        _service = new EmailService(settings, _smtpMock);
    }

    [Fact]
    public async Task SendEmailAsync_ValidData_ShouldCallSendOnClient()
    {
        var model = new Dictionary<string, string>
        {
            ["DISPLAY_NAME"] = "TestName",
            ["CODE"] = "123456",
            ["MINUTES"] = "5"
        };
        await _service.SendEmailAsync("test@email.com", EmailTemplate.VerifyEmailCode, model);
        await _smtpMock.Received(1).SendAsync(Arg.Any<MimeMessage>());
    }

    [Fact]
    public async Task SendEmailAsync_ValidData_ModelValuesShouldBePresent()
    {
        var model = new Dictionary<string, string>
        {
            ["DISPLAY_NAME"] = "TestName",
            ["CODE"] = "654321",
            ["MINUTES"] = "10"
        };
        
        await _service.SendEmailAsync("JohnDoe@test.com",  EmailTemplate.VerifyEmailCode, model);
        
        await _smtpMock.Received(1).SendAsync(Arg.Is<MimeMessage>(msg =>
            msg.HtmlBody.Contains("TestName") &&
            msg.HtmlBody.Contains("654321") &&
            msg.HtmlBody.Contains("10")));
    }

    [Fact]
    public async Task SendEmailAsync_ValidEmailTemplate_DoesNotContainHtmlTags()
    {
        var model = new Dictionary<string, string>
        {
            ["DISPLAY_NAME"] = "TestName",
            ["CODE"] = "555555",
            ["MINUTES"] = "5"
        }; 
        await _service.SendEmailAsync("JohnDoe@test.com",  EmailTemplate.VerifyEmailCode, model);
        
        await _smtpMock.Received(1).SendAsync(Arg.Is<MimeMessage>(msg => 
            !msg.TextBody.Contains("<p>") && 
            !msg.TextBody.Contains("</div>") &&
            msg.TextBody.Length > 0
        ));
    }

    [Fact]
    public async Task SendEmailAsync_ValidTemplate_SetsCorrectSubject()
    {
        var model = new Dictionary<string, string>
        {
            ["DISPLAY_NAME"] = "TestName",
            ["CODE"] = "111111",
            ["MINUTES"] = "12"
        }; 
        
        await _service.SendEmailAsync("JohnDoe@test.com", EmailTemplate.VerifyEmailCode, model);
        
        await _smtpMock.Received(1).SendAsync(Arg.Is<MimeMessage>(msg => 
                msg.Subject != null && 
                msg.Subject.Equals("Your Outstanding verification code") &&
                !msg.Subject.StartsWith("Subject:")
        ));
    }
    
    [Fact]
    public async Task SendEmailAsync_TemplateFileNotFound_ShouldThrowFileNotFoundException()
    {
        var model = new Dictionary<string, string>
        {
            ["DISPLAY_NAME"] = "TestName",
            ["CODE"] = "111111",
            ["MINUTES"] = "12"
        }; 
        
        var invalidTemplate = (EmailTemplate)1;
        
        await Assert.ThrowsAsync<FileNotFoundException>(() => 
            _service.SendEmailAsync("JohnDoe@test.com", invalidTemplate, model));
    }
}