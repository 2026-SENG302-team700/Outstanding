using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SENG302.Api.Services;
using Shouldly;
using Xunit;


namespace SENG302.Api.Tests.Integration.Services;

public class EmailServiceTests : BaseIntegrationTestFixture
{
    private IEmailService ServiceUnderTest => ServiceProvider.GetRequiredService<IEmailService>();

    public EmailServiceTests(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }

    [Fact]
    public async Task SendEmail_ToLocalSmtp_DoesNotThrow()
    {
        var settings = Options.Create(new EmailSettings
        {
            Host = "localhost",
            Port = 1025,
            FromEmail = "noreply.outstanding@gmail.com",
            Password = ""
        });

        await Should.NotThrowAsync(async () =>
        {
            await ServiceUnderTest.SendEmailAsync(
                "test@example.com",
                EmailTemplate.VerifyEmailCode,
                new Dictionary<string, string>
                {
                    ["DISPLAY_NAME"] = "Test",
                    ["CODE"] = "123456",
                    ["MINUTES"] = "5"
                });
        });
    }
}