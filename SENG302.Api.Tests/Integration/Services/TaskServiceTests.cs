using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
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
}
