using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;

namespace SENG302.Api.Tests.Integration.Services;

public class TaskServiceTests : BaseIntegrationTestFixture
{
    private ITaskListService ServiceUnderTest => ServiceProvider.GetRequiredService<ITaskListService>();

    public TaskServiceTests(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }

    [Theory]
    [InlineData("test", "test1@example.com")] // Basic Test
    [InlineData("abc", "test2@example.com")] // Name with exactly 3 characters
    [InlineData("  xyz  ", "test3@example.com")] // Name with 3 characters but whitespace that will be trimmed
    [InlineData("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890abcdefgh", "test4@example.com")] // Name with 128 characters
    [InlineData(" 123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890abcdefgh ", "test5@example.com")] // Name with 128 characters but whitespace that should be trimmed
    [InlineData("f                  f", "test6@example.com")] // Lots of spaces but only a few characters at either side
    public async Task CreateNewTaskList_Success_ReturnList(string name, string userEmail)
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

        taskList.Name.ShouldBe(name.Trim());
        taskList.UserEmail.ShouldBe(userEmail);
    }

    [Theory]
    [InlineData("Hi", "test4@example.com")] // Short name that is less than 3 characters
    [InlineData("Hi!", "test5@example.com")] // Name with special character
    [InlineData("                    Hi  ", "test6@example.com")] // long enough but whitespace should be trimmed, meaning not long enough
    [InlineData("          ", "test7@example.com")] // long enough but only whitespace
    [InlineData("x123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890abcdefgh ", "test5@example.com")] // Name with 129 characters so invalid
    public async Task CreateNewTaskList_InvalidName_ThrowArgumentException(string name, string userEmail)
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
