/*
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;

namespace SENG302.Api.Tests.Unit.Services;

public class TaskServiceUnitTest : BaseUnitTestFixture
{
    private ITaskService ServiceUnderTest => ServiceProvider.GetRequiredService<ITaskService>();

    public TaskServiceUnitTest(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }

    [Theory]
    [InlineData("test", "test1@example.com")] // Basic Test
    [InlineData("abc", "test2@example.com")] // Name with exactly 3 characters
    public async Task CreateNewTaskList_Success_ReturnList(String name, String userEmail)
    {
        // Add a user to the database with the email that we are testing with
        await using var context = DbContextFactory.CreateDbContext();
        context.Users.Add(new User
        {
            Email = userEmail,
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        await context.SaveChangesAsync();

        // Use the TaskService function to create a new task list with the name and user email
        var taskList = await ServiceUnderTest.CreateNewTaskListAsync(name, userEmail);

        taskList.Name.ShouldBe(name);
        taskList.UserEmail.ShouldBe(userEmail);
    }

    [Theory]
    [InlineData("Hi", "test4@example.com")] // Short name that is less than 3 characters
    [InlineData("Hi!", "test5@example.com")] // Name with special character
    public async Task CreateNewTaskList_InvalidName_ThrowArgumentException(String name, String userEmail)
    {
        // Add a user to the database with the email that we are testing with
        await using var context = DbContextFactory.CreateDbContext();
        context.Users.Add(new User
        {
            Email = userEmail,
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        await context.SaveChangesAsync();

        // Use the TaskService function to create a new task list with the name and user email and check that it throws an ArgumentException
        await Should.ThrowAsync<ArgumentException>(async () => await ServiceUnderTest.CreateNewTaskListAsync(name, userEmail));
    }


    [Theory]
    [InlineData("bob mcnugg", "testing", 2022, 04, 23, CurrentTaskStatus.Todo)]
    public async Task CreateNewTaskItemAsync_InvalidDate_ThrowArgumentException(string name, string description, int year, int month, int day, CurrentTaskStatus currentStatus)
    {
        await using var context = DbContextFactory.CreateDbContext();

        context.Users.Add(new User
        {
            Email = "fish@ocean.com",
            DisplayName = "fish",
            PasswordKey = "averysecurepassword",
            Country = "The Atlantic",
        });

        context.TaskLists.Add(new TaskList
        {
            Id = 3,
            Name = "Testlist",
            UserEmail = "fish@ocean.com"

        });
        NewTaskItemRequest newTask = new NewTaskItemRequest
        {
            TaskListId = 3,
            Name = name,
            Description = description,
            DueDate = new DateTime(year, month, day),
            CurrentStatus = currentStatus,
        };
        await context.SaveChangesAsync();
        await Should.ThrowAsync<ArgumentException>(async () => await ServiceUnderTest.CreateNewTaskItemAsync(newTask));
    }

    //generates a string 10 times the size of loop
    private string generateString(int loops)
    {
        string value = "";
        for (int i = 0; i < loops; i++)
        {
            value += "aaaaaaaaaa";
        }
        return value;
    }

    [Theory]
    [InlineData("bob", "", CurrentTaskStatus.Todo, 2027, 04, 23)]
    [InlineData(" ", "here is a lovely description!", CurrentTaskStatus.Todo, 6064, 04, 23)]
    [InlineData("bob mcnugg", "", CurrentTaskStatus.Todo, 9998, 01, 31)]
    [InlineData("meet angie @ the bar /w bob", " ", CurrentTaskStatus.Todo, 2027, 04, 23)]
    [InlineData("meet angie @ the bar /w bob", " ", CurrentTaskStatus.Todo)]
    [InlineData(" ", "", CurrentTaskStatus.Todo)]
    public async Task CreateNewTaskItemAsync_CreateTask_Success(string name, string description, CurrentTaskStatus currentStatus, int year = 0, int month = 0, int day = 0)
    {
        await using var context = DbContextFactory.CreateDbContext();

        context.Users.Add(new User
        {
            Email = "fish@ocean.com",
            DisplayName = "fish",
            PasswordKey = "averysecurepassword",
            Country = "The Atlantic",
        });
        context.TaskLists.Add(new TaskList
        {
            Id = 3,
            Name = "Testlist",
            UserEmail = "fish@ocean.com"
        });
        if (description == " ")
        {
            description = generateString(200);
        }
        else if (name == " ")
        {
            name = generateString(10);
        }
        //sets due date to be zeroed out, but is changed to it's proper time if the year is not 0
        var dueDate = new DateTime(0001, 01, 01);
        if (year > 0)
        {
            dueDate = new DateTime(year, month, day);
        }
        NewTaskItemRequest newTask = new NewTaskItemRequest
        {
            TaskListId = 3,
            Name = name,
            Description = description,
            DueDate = dueDate,
            CurrentStatus = currentStatus,
        };
        await context.SaveChangesAsync();
        var createdTask = await ServiceUnderTest.CreateNewTaskItemAsync(newTask);
        if (description == "")
        {
            createdTask.Description.ShouldBe("No Description");
        }
        else
        {
            createdTask.Description.ShouldBe(description);
        }
    }
}
*/