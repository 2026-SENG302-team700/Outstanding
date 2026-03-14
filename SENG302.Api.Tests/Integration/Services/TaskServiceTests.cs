using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;

namespace SENG302.Api.Tests.Integration.Services;

public class TaskServiceTests : BaseIntegrationTestFixture
{
    private ITaskService ServiceUnderTest => ServiceProvider.GetRequiredService<ITaskService>();

    public TaskServiceTests(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }

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

    [Fact]
    public async Task GetTaskListsByUserEmailAsync_RetreiveUsersTaskLists_Success()
    {
        await using var context = DbContextFactory.CreateDbContext();
        // Add a user to the database with the email that we are testing with
        context.Users.Add(new User
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordKey = "password",
            Country = "Test Country"
        });
        await context.SaveChangesAsync();

        // Use the TaskService function to retrieve task lists for the user email
        var taskLists = await ServiceUnderTest.GetTaskListsByUserEmailAsync("test@example.com");
        taskLists.Count().ShouldBe(0);

        context.TaskLists.Add(new TaskList
        {
            Name = "Test Task List",
            UserEmail = "test@example.com"
        });
        await context.SaveChangesAsync();
        var updatedTaskLists = await ServiceUnderTest.GetTaskListsByUserEmailAsync("test@example.com");
        updatedTaskLists.Count().ShouldBe(1);
        updatedTaskLists.First().Name.ShouldBe("Test Task List");
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

    [Theory]
    [InlineData("b", "this is a fantastic description regarding this failing task!", 6064, 04, 23, CurrentTaskStatus.Todo)]
    [InlineData("", "", 6064, 04, 23, CurrentTaskStatus.Todo)]
    [InlineData("bob mcnugg", "", 1900, 04, 23, CurrentTaskStatus.Todo)]
    [InlineData("bob mcnugg", " ", 2027, 04, 23, CurrentTaskStatus.Todo)]
    [InlineData(" ", "", 2027, 04, 23, CurrentTaskStatus.Todo)]
    public async Task CreateNewTaskItemAsync_InvalidData_ThrowArgumentException(string name, string description, int year, int month, int day, CurrentTaskStatus currentStatus) {
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
            Name = "Testlist",
            UserEmail = "fish@ocean.com"
        });
        if (description == " ")
        {
            description = generateString(210);
        } 
        else if (name == " ") {
            name = generateString(13);
        }
        NewTaskItemRequest newTask = new NewTaskItemRequest 
        {
            TaskListId = 0,
            Name = name,
            Description = description,
            DueDate = new DateTime(year, month, day),
            CurrentStatus = currentStatus,
        };
        await context.SaveChangesAsync();
        await Should.ThrowAsync<ArgumentException>(async () => await ServiceUnderTest.CreateNewTaskItemAsync(newTask));
    }
    [Theory]
    [InlineData("bob", "",CurrentTaskStatus.Todo, 2027, 04, 23)]
    [InlineData(" ", "here is a lovely description!", CurrentTaskStatus.Todo, 6064, 04, 23)]
    [InlineData("bob mcnugg", "",CurrentTaskStatus.Todo, 9998, 01, 31)]
    [InlineData("meet angie @ the bar /w bob", " ",CurrentTaskStatus.Todo, 2027, 04, 23)]
    [InlineData("meet angie @ the bar /w bob", " ",CurrentTaskStatus.Todo)]
    [InlineData(" ", "",  CurrentTaskStatus.Todo)]
    public async Task CreateNewTaskItemAsync_CreateTask_Success(string name, string description, CurrentTaskStatus currentStatus, int year = 0, int month = 0, int day = 0) {
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
            Name = "Testlist",
            UserEmail = "fish@ocean.com"
        });
        if (description == " ")
        {
            description = generateString(200);
        } 
        else if (name == " ") {
            name = generateString(10);
        }
        //sets due date to be zeroed out, but is changed to it's proper time if the year is not 0
        var dueDate = new DateTime(0001,01,01);
        if (year > 0) {
            dueDate = new DateTime(year, month, day);
        } 
        NewTaskItemRequest newTask = new NewTaskItemRequest 
        {
            TaskListId = 1,
            Name = name,
            Description = description,
            DueDate = dueDate,
            CurrentStatus = currentStatus,
        };
        await context.SaveChangesAsync();
        var createdTask = await ServiceUnderTest.CreateNewTaskItemAsync(newTask);
        createdTask.Description.ShouldBe(description);
    }

}
