using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SENG302.Api.Services;
using Shouldly;

namespace SENG302.Api.Tests.Integration.Services;

public class EmailServiceTests : BaseIntegrationTestFixture
{
    private IEmailService ServiceUnderTest => ServiceProvider.GetRequiredService<IEmailService>();

    public EmailServiceTests(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }
}