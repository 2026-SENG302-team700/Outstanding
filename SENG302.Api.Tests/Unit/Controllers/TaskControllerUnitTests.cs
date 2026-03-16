using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SENG302.Api.Models.Requests;
using SENG302.Api.Models.Entities;
using Shouldly;

namespace SENG302.Api.Tests.Unit.Controllers;

public class TaskControllerUnitTests : BaseUnitTestFixture
{
    public TaskControllerUnitTests(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory) { }

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
    public async Task GetTasksFromList_SuccessfulFetch_ReturnTaskItems()
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

        // Create task list for user
        var data = new NewTaskItemRequest
        {
            Name = "Test Task",
            Description = "test",
            TaskListId = 1,
        };
        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", data);
        message.StatusCode.ShouldBe(HttpStatusCode.OK); // Ensure task list creation was successful

        // Fetch task lists for the user
        message = await HttpClient.GetAsync($"/api/taskItem/{1}");
        message.StatusCode.ShouldBe(HttpStatusCode.OK); // Ensure fetching task lists was successful
        TaskItem[] taskItems = JsonConvert.DeserializeObject<TaskItem[]>(await message.Content.ReadAsStringAsync())!;
        taskItems.Length.ShouldBe(1);
    }

    //generates a string 10 times the size of loop
    private string generateString(int loops)
    {
        string value = "";
        for (int i = 0; i <loops; i++)
        {
            value += "aaaaaaaaaa";
        }
        return value;
    }
    [Fact]
    public async Task CreateTaskItem_EmptyTaskName_BadRequest()
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
        var data = new NewTaskItemRequest
        {
            Name = "",
            Description = "test",
            TaskListId = 1,
        };

        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", data);
        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTaskItem_ShortTaskName_BadRequest()
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
        var data = new NewTaskItemRequest
        {
            Name = "b",
            Description = "test",
            TaskListId = 1,
        };

        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", data);
        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTaskItem_LongTaskName_BadRequest()
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
        var name = generateString(13);// generates a string of 130 characters
        var data = new NewTaskItemRequest
        {
            Name = name,
            Description = "test",
            TaskListId = 1,
        };

        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", data);
        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTaskItem_LongDescription_BadRequest()
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
        var description = generateString(210);// generates a string of 2100 characters
        var data = new NewTaskItemRequest
        {
            Name = "bnob",
            Description = description,
            TaskListId = 1,
        };

        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", data);
        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTaskItem_InvalidTaskListId_BadRequest()
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
        var data = new NewTaskItemRequest
        {
            Name = "bob ",
            Description = "test",
            TaskListId = -1,
        };

        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", data);
        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTaskItem_IncorrectEmail_Unauthorized()
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
            UserEmail = "bob@example.com"
        });
        await context.SaveChangesAsync();
        var data = new NewTaskItemRequest
        {
            Name = "name",
            Description = "test",
            TaskListId = 1,
        };

        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", data);
        message.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTaskItem_NonExistentTaskList_BadRequest()
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
        var data = new NewTaskItemRequest
        {
            Name = "name",
            Description = "test",
            TaskListId = 10,
        };

        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", data);
        message.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

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
        var data = new NewTaskItemRequest
        {
            Name = "name",
            Description = "test",
            TaskListId = 1,
        };

        var message = await HttpClient.PostAsJsonAsync("/api/taskItem", data);
        message.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
}