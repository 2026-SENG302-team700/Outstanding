using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SENG302.Api.Models.Requests;
using SENG302.Api.Models.Entities;
using Shouldly;

namespace SENG302.Api.Tests.Integration.ControllerTests;

public class TaskListControllerTests : BaseIntegrationTestFixture
{
    public TaskListControllerTests(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

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
        };
        var message = await HttpClient.PostAsJsonAsync("/api/taskList", data);
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
        };
        var response = await HttpClient.PostAsJsonAsync("/api/taskList", data);
        var message = await response.Content.ReadAsStringAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        message.ShouldContain("List name is required and must be between 3 and 128 characters long");
    }

    [Fact]
    public async Task CreateTaskList_NonExistentUser_ReturnBadRequest()
    {
        // Test automatically gets userEamil as test@example.com so we dont add this
        // to the db for this test
        var data = new NewTaskListRequest
        {
            Name = "Valid Task List Name",
        };
        var response = await HttpClient.PostAsJsonAsync("/api/taskList", data);
        var message = await response.Content.ReadAsStringAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        message.ShouldContain("User with the provided id does not exist.");
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
        };
        var response = await HttpClient.PostAsJsonAsync("/api/taskList", data);
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
        };
        var response = await HttpClient.PostAsJsonAsync("/api/taskList", data);

        // Fetch task lists for the user
        response = await HttpClient.GetAsync("/api/taskList");
        response.StatusCode.ShouldBe(HttpStatusCode.OK); // Ensure fetching task lists was successful
        TaskList[] taskLists = await response.Content.ReadFromJsonAsync<TaskList[]>() ?? Array.Empty<TaskList>();
        taskLists.Length.ShouldBe(1);
    }
}