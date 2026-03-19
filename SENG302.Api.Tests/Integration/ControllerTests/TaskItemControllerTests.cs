using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SENG302.Api.Models.Requests;
using SENG302.Api.Models.Entities;
using Shouldly;
using System.Runtime.InteropServices;

namespace SENG302.Api.Tests.Integration.ControllerTests;

public class TaskItemControllerTests : BaseIntegrationTestFixture
{
    public TaskItemControllerTests(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

    [Fact]
    public async Task CreateTaskItem_ValidData_Ok()
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
        context.TaskLists.Add(new TaskList
        {
            Id = 1,
            Name = "test tasklist",
            UserEmail = "test@example.com"
        });
        await context.SaveChangesAsync();

        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", new
        {
            taskListId = 1,
            name = "name",
            description = "test",
            dueDate = "2030-01-01",
            currentTaskStatus = 0
        });
        message.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("aa")] // Short - exactly 2 chars
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // Long - 129 chars
    [InlineData("")] // No string
    [InlineData("   ")] // Made up of whitespace
    [InlineData("Hi ")] // Padded with whitespace
    public async Task CreateTaskItem_InvalidName_BadRequest(string name)
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
        context.TaskLists.Add(new TaskList
        {
            Id = 1,
            Name = "test tasklist",
            UserEmail = "test@example.com"
        });
        await context.SaveChangesAsync();

        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", new
        {
            taskListId = 1,
            name = name,
            description = "test",
            dueDate = "2030-01-01",
            currentTaskStatus = 0
        });
        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTaskItem_NoDescription_DefaultDescriptionSet()
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
        context.TaskLists.Add(new TaskList
        {
            Id = 1,
            Name = "test tasklist",
            UserEmail = "test@example.com"
        });
        await context.SaveChangesAsync();

        var response = await HttpClient.PostAsJsonAsync("/api/taskItem", new
        {
            taskListId = 1,
            name = "test",
            description = "",
            dueDate = "2030-01-01",
            currentTaskStatus = 0
        });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var taskItem = await response.Content.ReadFromJsonAsync<TaskItem>();
        taskItem!.Description.ShouldBe("No Description");
    }

    [Theory]
    [InlineData("2000-01-01")] // due date in the past
    public async Task CreateTaskItem_InvalidDate_BadRequest(string date)
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
        context.TaskLists.Add(new TaskList
        {
            Id = 1,
            Name = "test tasklist",
            UserEmail = "test@example.com"
        });
        await context.SaveChangesAsync();

        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", new
        {
            taskListId = 1,
            name = "test name",
            description = "test",
            dueDate = date,
            currentTaskStatus = 0
        });
        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}