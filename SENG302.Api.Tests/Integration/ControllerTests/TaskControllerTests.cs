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

    [Fact]
    public async Task CreateTaskList_ShortName_ReturnBadRequest()
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
            Name = "ab", // Short name that is less than 3 characters
            UserEmail = "test@example.com"
        };
        var response = await HttpClient.PostAsJsonAsync("/api/tasks", data);
        var message = await response.Content.ReadAsStringAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        message.ShouldContain("List name is required and must be between 3 and 128 characters long");
    }

    [Fact]
    public async Task CreateTaskList_NonExistentUser_ReturnBadRequest()
    {
        var data = new NewTaskListRequest
        {
            Name = "Valid Task List Name",
            UserEmail = "nonexistent@example.com"
        };
        var response = await HttpClient.PostAsJsonAsync("/api/tasks", data);
        var message = await response.Content.ReadAsStringAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        message.ShouldContain("User with the provided email does not exist.");
    }

    [Fact]
    public async Task CreateTaskList_InvalidCharactersInName_ReturnBadRequest()
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
            Name = "test!", // Invalid character in name
            UserEmail = "test@example.com"
        };
        var response = await HttpClient.PostAsJsonAsync("/api/tasks", data);
        var message = await response.Content.ReadAsStringAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        message.ShouldContain("List name cannot contain characters other than letters, spaces, hyphens, apostrophes, or numbers");
    }

    [Fact]
    public async Task FetchTaskListsByUser_SuccessfulFetch_ReturnsTaskLists()
    {
        // Create DB
        await using var context = DbContextFactory.CreateDbContext();

        // Add user to DB
        context.Users.Add(new User
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        await context.SaveChangesAsync();

        // Create task list for user
        var data = new NewTaskListRequest
        {
            Name = "Test Task List",
            UserEmail = "test@example.com"
        };
        var message = await HttpClient.PostAsJsonAsync("/api/tasks", data);
        //message.StatusCode.ShouldBe(HttpStatusCode.OK); // Ensure task list creation was successful

        // Fetch task lists for the user
        message = await HttpClient.GetAsync("/api/tasks");
        message.StatusCode.ShouldBe(HttpStatusCode.OK); // Ensure fetching task lists was successful
        TaskList[] taskLists = JsonConvert.DeserializeObject<TaskList[]>(await message.Content.ReadAsStringAsync())!;
        taskLists.Length.ShouldBe(1);
    }
}