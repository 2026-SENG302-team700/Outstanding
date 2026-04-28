using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SENG302.Api.Models.Entities;
using Shouldly;

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
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        context.TaskLists.Add(new TaskList
        {
            Id = 1,
            Name = "test tasklist",
            UserId = 1
        });
        await context.SaveChangesAsync();

        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", new
        {
            taskListId = 1,
            name = "name",
            description = "test",
            dueDate = DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
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
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        context.TaskLists.Add(new TaskList
        {
            Id = 1,
            Name = "test tasklist",
            UserId = 1
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
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        context.TaskLists.Add(new TaskList
        {
            Id = 1,
            Name = "test tasklist",
            UserId = 1
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
        taskItem!.Description.ShouldBe("");
    }

    [Fact]
    public async Task CreateTaskItem_PastDate_BadRequest()
    {
        // Create DB
        await using var context = DbContextFactory.CreateDbContext();

        // Add user to DB
        context.Users.Add(new User
        {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        context.TaskLists.Add(new TaskList
        {
            Id = 1,
            Name = "test tasklist",
            UserId = 1
        });
        await context.SaveChangesAsync();

        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", new
        {
            taskListId = 1,
            name = "test name",
            description = "test",
            dueDate = "2000-01-01",
            currentTaskStatus = 0
        });
        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTaskItem_Valid_ReturnsOkAndUpdatedTaskItem()
    {
        await using var context = DbContextFactory.CreateDbContext();
        context.Users.Add(new User { 
            Id = 1,
            Email = "vlad@nistor.email", 
            DisplayName = "Vlad Nistor", 
            PasswordKey = "password", 
            Country = "RO" });                                                                                                                      
        context.TaskLists.Add(new TaskList
        {
            Id = 1, 
            Name = "tasklist", 
            UserId = 1
        });                            
        context.TaskItems.Add(new TaskItem { 
            TaskId = 1, 
            TaskListId = 1,
            Name = "mercedes", 
            Description = "lewis hamilton f1 team", 
            CurrentStatus = CurrentTaskStatus.Todo 
        });                                                                                                    
        await context.SaveChangesAsync();

        var response = await HttpClient.PutAsJsonAsync("/api/taskItem/item/1", new
        {
            taskId = 1,
            name = "ferrari",
            description = "lewis hamilton new f1 team",
            dueDate = DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
            currentStatus = 1
        });
        
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var task = await response.Content.ReadFromJsonAsync<TaskItem>();
        task!.Name.ShouldBe("ferrari");
        task.Description.ShouldBe("lewis hamilton new f1 team");
        task.CurrentStatus.ShouldBe(CurrentTaskStatus.InProgress);
    }
    
    [Theory]
    [InlineData("aa")]
    [InlineData("   ")]
    [InlineData("aa    ")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task UpdateTaskItem_InvalidName_ReturnsBadRequest(string badName)
    { 
        await using var context = DbContextFactory.CreateDbContext();
        context.Users.Add(new User { 
            Id = 1,
            Email = "vlad@nistor.email", 
            DisplayName = "Vlad Nistor", 
            PasswordKey = "password", 
            Country = "RO" });                                                                                                                      
        
        context.TaskLists.Add(new TaskList 
        { 
            Id = 1, 
            Name = "tasklist", 
            UserId = 1
        });
        
        context.TaskItems.Add(new TaskItem { 
            TaskId = 1, 
            TaskListId = 1, 
            Name = "goodName", 
            Description = "", 
            CurrentStatus = CurrentTaskStatus.Todo 
        }); 
        await context.SaveChangesAsync();
        
        
        
        var response = await HttpClient.PutAsJsonAsync("/api/taskItem/item/1", new 
        { 
            taskId = 1, 
            name = badName, 
            description = "", 
            dueDate = DateTime.UtcNow.Add(TimeSpan.FromDays(1)), 
            currentStatus = 1
        });
            
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTaskItem_InvalidDescription_ReturnsBadRequest()
    { 
        await using var context = DbContextFactory.CreateDbContext();
        context.Users.Add(new User { 
            Id = 1,
            Email = "vlad@nistor.email", 
            DisplayName = "Vlad Nistor", 
            PasswordKey = "password", 
            Country = "RO" });                                                                                                                      
        
        context.TaskLists.Add(new TaskList 
        { 
            Id = 1, 
            Name = "tasklist", 
            UserId = 1
            
        });
        
        context.TaskItems.Add(new TaskItem { 
            TaskId = 1, 
            TaskListId = 1, 
            Name = "goodName", 
            Description = "", 
            CurrentStatus = CurrentTaskStatus.Todo 
        }); 
        await context.SaveChangesAsync();
        
        
        
        var response = await HttpClient.PutAsJsonAsync("/api/taskItem/item/1", new 
        { 
            taskId = 1, 
            name = "goodName", 
            description = new string ('a', 2049), 
            dueDate = DateTime.UtcNow.Add(TimeSpan.FromDays(1)), 
            currentStatus = 1
        });
            
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}