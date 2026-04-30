using System.Data.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using SENG302.Api.Tests.Integration;
using Reqnroll;
using SENG302.Api;
using SENG302.Api.DataAccess;

namespace SENG302.Api.Tests.Acceptance.Setup;

[Binding]
public class AcceptanceTestFixture : BaseIntegrationTestFixture
{
    public int? CurrentUserId { get; set; }
    public new HttpClient HttpClient => base.HttpClient;
    public new IDbContextFactory<DatabaseContext> DbContextFactory => base.DbContextFactory;

    public AcceptanceTestFixture(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [BeforeScenario]
    public async Task ScenarioSetup()
    {
        await InitializeAsync();
        CurrentUserId = null;
    }

    [AfterScenario]
    public async Task ScenarioTeardown()
    {
        await DisposeAsync();
    }
}
