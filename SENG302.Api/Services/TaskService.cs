using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SENG302.Api.Services;

public interface ITaskService
{
    Task<TaskList> CreateNewTaskListAsync(string name, string userEmail);
    Task<TaskList> GetTaskListByIdAsync(int id);

}

public class TaskService : ITaskService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly TimeProvider _timeProvider;

    public TaskService(IDbContextFactory<DatabaseContext> dbContextFactory, TimeProvider timeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _timeProvider = timeProvider;
    }

    public async Task<TaskList> CreateNewTaskListAsync(string name, string userEmail)
    {
        // Get a database context
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        // Validate the name
        if (string.IsNullOrEmpty(name) || name.Length < 3 || name.Length > 128)
        {
            throw new ArgumentException("Name must be between 3 and 128 characters.");
        }

        // Validate the user email
        var user = await context.Users.Where(u => u.Email == userEmail).FirstOrDefaultAsync();
        if (user == null)
        {
            throw new ArgumentException("User with the provided email does not exist.");
        }

        // Create the new task list
        var newTaskList = new TaskList()
        {
            Name = name,
            UserEmail = userEmail
        };
        context.Set<TaskList>().Add(newTaskList);
        await context.SaveChangesAsync();
        return newTaskList;
    }

    public async Task<TaskList> GetTaskListByIdAsync(int id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var taskList = await context.Set<TaskList>().Where(t => t.Id == id).FirstOrDefaultAsync();

        return taskList;
    }
}