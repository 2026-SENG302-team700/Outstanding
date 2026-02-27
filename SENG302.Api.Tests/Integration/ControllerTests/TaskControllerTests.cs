using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SENG302.Api.Models.Requests;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;

namespace SENG302.Api.Tests.Integration.ControllerTests;

public class TaskControllerTests : BaseIntegrationTestFixture
{
    public TaskControllerTests(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

    private ITaskService ServiceUnderTest => ServiceProvider.GetRequiredService<ITaskService>();


    [Fact]
    public async Task CreateTaskList_SuccessfulCreation_ReturnOk()
    {
        await using var context = DbContextFactory.CreateDbContext();
        context.Users.Add(new User
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        await context.SaveChangesAsync();

        var data = new NewTaskListRequest
        {
            Name = "Test Task List",
            UserEmail = "test@example.com"
        };
        var message = await HttpClient.PostAsJsonAsync("/api/tasks", data);
        message.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}