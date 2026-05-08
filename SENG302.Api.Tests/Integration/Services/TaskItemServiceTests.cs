using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using SENG302.Api.Services;
using Shouldly;

namespace SENG302.Api.Tests.Integration.Services;

public class TaskItemServiceTests : BaseIntegrationTestFixture
{
    private ITaskItemService ServiceUnderTest => ServiceProvider.GetRequiredService<ITaskItemService>();
    public TaskItemServiceTests(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }

    private async Task SetupUserAndList(int userId, int listId)
    {
        await using var context = DbContextFactory.CreateDbContext();
        context.Users.Add(new User
        {
            Id = userId,
            Email = $"test{userId}@test.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        context.TaskLists.Add(new TaskList
        {
            Id = listId,
            Name = "Test List",
            UserId = userId
        });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllTaskItemsAsync_NoTasks_ReturnsEmpty()
    {
        await SetupUserAndList(1, 1);
        var tasks = await ServiceUnderTest.GetAllTaskItemsAsync(1);
        tasks.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllTaskItemsAsync_WithTasks_ReturnsOnlyUsersTask()
    {
        // setup two users and a task
        await SetupUserAndList(1, 1);
        await SetupUserAndList(2, 2);

        await using var context = DbContextFactory.CreateDbContext();
        context.TaskItems.Add(new TaskItem
        {
            TaskListId = 1,
            Name = "User 1 task",
            Description = "desc",
            CurrentStatus = CurrentTaskStatus.Todo,
            creationTime = DateTimeOffset.UtcNow
        });
        context.TaskItems.Add(new TaskItem
        {
            TaskListId = 2,
            Name = "User 2 task",
            Description = "desc",
            CurrentStatus = CurrentTaskStatus.Todo,
            creationTime = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        var tasks = await ServiceUnderTest.GetAllTaskItemsAsync(1);

        tasks.Count().ShouldBe(1);
        tasks.First().Name.ShouldBe("User 1 task");
    }

    [Fact]
    public async Task CreateNewTaskItemAsync_ValidRequest_ReturnsCreatedTask()
    {
        await SetupUserAndList(1, 1);

        var request = new NewTaskItemRequest
        {
            TaskListId = 1,
            Name = "New task",
            Description = "Some description",
            CurrentStatus = CurrentTaskStatus.Todo,
            DueDate = null
        };

        var task = await ServiceUnderTest.CreateNewTaskItemAsync(request, 1);

        task.Name.ShouldBe("New task");
        task.TaskListId.ShouldBe(1);
        task.CurrentStatus.ShouldBe(CurrentTaskStatus.Todo);
    }

    [Theory]
    [InlineData("hi")] // too short
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // too long
    public async Task CreateNewTaskItemAsync_InvalidName_ThrowsValidationException(string name)
    {
        await SetupUserAndList(1, 1);

        var request = new NewTaskItemRequest
        {
            TaskListId = 1,
            Name = name,
            Description = "desc",
            CurrentStatus = CurrentTaskStatus.Todo,
            DueDate = null
        };

        await Should.ThrowAsync<MultipleValidationException>(
            async () => await ServiceUnderTest.CreateNewTaskItemAsync(request, 1));
    }

    [Fact]
    public async Task CreateNewTaskItemAsync_PastDueDate_ThrowsValidationException()
    {
        await SetupUserAndList(1, 1);

        var request = new NewTaskItemRequest
        {
            TaskListId = 1,
            Name = "Valid",
            Description = "desc",
            CurrentStatus = CurrentTaskStatus.Todo,
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        await Should.ThrowAsync<MultipleValidationException>(
            async () => await ServiceUnderTest.CreateNewTaskItemAsync(request, 1));
    }

    [Fact]
    public async Task GetTaskItemAsync_ValidId_ReturnsTask()
    {
        await SetupUserAndList(1, 1);

        await using var context = DbContextFactory.CreateDbContext();
        var task = new TaskItem
        {
            TaskListId = 1,
            Name = "Fetch me",
            Description = "desc",
            CurrentStatus = CurrentTaskStatus.Todo,
            creationTime = DateTimeOffset.UtcNow
        };
        context.TaskItems.Add(task);
        await context.SaveChangesAsync();

        var result = await ServiceUnderTest.GetTaskItemAsync(task.TaskId, 1);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Fetch me");
    }

    [Fact]
    public async Task GetTaskItemAsync_InvalidId_ReturnsNull()
    {
        var result = await ServiceUnderTest.GetTaskItemAsync(-1, 1);
        result.ShouldBeNull();
    }
    
    [Fact]
    public async Task ReorderTaskItemsAsync_ValidOrdering_SetsOrderingPosition()
    {
        await SetupUserAndList(1, 1);
        
        await using var context = DbContextFactory.CreateDbContext();
        context.TaskItems.AddRange(
            new TaskItem {
                TaskId = 1,
                TaskListId = 1,
                Name = "task 1",
                Description = ""
            },
            new TaskItem {
                TaskId = 2,
                TaskListId = 1,
                Name = "task 2",
                Description = ""
            },
            new TaskItem {
                TaskId = 3,
                TaskListId = 1,
                Name = "task 3",
                Description = ""
            }
        );
        
        await context.SaveChangesAsync();

        await ServiceUnderTest.ReorderTaskItemsAsync([3, 1, 2]);
        var taskItems = (await ServiceUnderTest.GetTaskItemsByListAsync(1)).ToList();
        
        taskItems.Count.ShouldBe(3);
        taskItems[0].TaskId.ShouldBe(3);
        taskItems[1].TaskId.ShouldBe(1);
        taskItems[2].TaskId.ShouldBe(2);
    }

    [Fact]
    public async Task ReorderTaskItemsAsync_NoTaskItems_NoChangesInDB()
    {
        await SetupUserAndList(1, 1);
        
        await using var context = DbContextFactory.CreateDbContext();
        context.TaskItems.Add(
            new TaskItem
            {
                TaskId = 1,
                TaskListId = 1,
                Name = "task 1",
                Description = "",
                OrderPosition = 8
            }
        );
        await context.SaveChangesAsync();
        
        await ServiceUnderTest.ReorderTaskItemsAsync([]);

        var taskItem = await ServiceUnderTest.GetTaskItemAsync(1, 1);
        taskItem.OrderPosition.ShouldBe(8);
    }
}