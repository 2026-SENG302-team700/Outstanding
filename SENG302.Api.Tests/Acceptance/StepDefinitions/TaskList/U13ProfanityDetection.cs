using System.Net;
using System.Net.Http.Json;
using Reqnroll;
using Shouldly;
using SENG302.Api.Tests.Acceptance.Setup;
using SENG302.Api.Models.Entities;

namespace SENG302.Api.Tests.Acceptance.StepDefinitions.TaskList;

[Binding]
public class U13ProfanityDetection
{
    private readonly AcceptanceTestFixture _fixture;
    private HttpResponseMessage? _lastResponse;

    public U13ProfanityDetection(AcceptanceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Given(@"I am a registered user")]
    public async Task GivenIAmARegisteredUser()
    {
        await using var context = await _fixture.DbContextFactory.CreateDbContextAsync();
        context.Users.Add(new User
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test user",
            PasswordKey = "password",
            Country = "NZ",
            ProfanityFiltering = false
        });
        await context.SaveChangesAsync();
        _fixture.CurrentUserId = 1;
    }

    [Given(@"I have the profanity filter enabled")]
    public async Task GivenIHaveTheProfanityFilterEnabled()
    {
        await using var context = await _fixture.DbContextFactory.CreateDbContextAsync();
        var user = await context.Users.FindAsync(1);
        user!.ProfanityFiltering = true;
        await context.SaveChangesAsync();
    }

    [Given(@"I have the profanity filter disabled")]
    public async Task GivenIHaveTheProfanityFilterDisabled()
    {
        await using var context = await _fixture.DbContextFactory.CreateDbContextAsync();
        var user = await context.Users.FindAsync(1);
        user!.ProfanityFiltering = false;
        await context.SaveChangesAsync();
    }

    [Given(@"I am on the create task list form")]
    public void GivenIAmOnTheCreateTaskListForm()
    {
        // no setup needed
    }

    [When(@"I create a task list with the name (.*)")]
    public async Task WhenICreateATaskListWithTheName(string name)
    {
        _lastResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/taskList", new
        {
            name = name.Trim()
        });
    }

    [Then(@"I should receive a bad request response")]
    public void ThenIShouldReceiveABadRequestResponse()
    {
        _lastResponse.ShouldNotBeNull();
        _lastResponse!.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Then(@"the error message should say {string}")]
    public async Task ThenTheErrorMessageShouldSay(string expectedMessage)
    {
        var message = await _lastResponse!.Content.ReadAsStringAsync();
        message.ShouldContain(expectedMessage);
    }

    [Then(@"The task list should be created")]
    public void ThenTheTaskListShouldBeCreated()
    {
        _lastResponse.ShouldNotBeNull();
        _lastResponse!.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}