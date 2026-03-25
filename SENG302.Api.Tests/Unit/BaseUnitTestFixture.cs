using Microsoft.AspNetCore.Mvc.Testing;
using SENG302.Api.Tests.Integration;

namespace SENG302.Api.Tests.Unit;

public abstract class BaseUnitTestFixture : BaseIntegrationTestFixture
{
    protected BaseUnitTestFixture(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }
}

