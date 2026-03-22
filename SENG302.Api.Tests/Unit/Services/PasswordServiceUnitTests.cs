using Microsoft.Extensions.Options;
using Xunit;
using NSubstitute;
using SENG302.Api.Services;
using MimeKit;

namespace SENG302.Api.Tests.Unit.Services;

public class PasswordServiceUnitTests
{
    private readonly ISmtpClientWrapper _smtpMock = Substitute.For<ISmtpClientWrapper>();
    private readonly EmailService _service;

    public PasswordServiceUnitTests()
    {
        var settings = Options.Create(new EmailSettings {
            FromEmail = "test@example.com",
            Host = "smtp.test.com"
        });
        // inject the mock into the real service
        _service = new EmailService(settings, _smtpMock);
    }

    public async Task 
}