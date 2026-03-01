using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SENG302.Api.Services;

public interface ITaskService
{
    Task<TaskList> CreateNewTaskListAsync(string name, string userEmail);
    Task<TaskList> GetTaskListByIdAsync(int id);
    Task<IEnumerable<TaskList>> GetTaskListsByUserEmailAsync(string userEmail);

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

    /// <summary>
    /// Gets all task lists associated with a user's email.
    /// </summary>
    /// <param name="userEmail"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TaskList>> GetTaskListsByUserEmailAsync(string userEmail)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var taskLists = await context.Set<TaskList>().Where(t => t.UserEmail == userEmail).ToListAsync();

        return taskLists;
    }

    /// <summary>
    /// Creates a new task list for a user with the given email and name. Validates the 
    /// name and user email before creating the task list.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="userEmail"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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

    /// <summary>
    /// Gets a task list by its ID. Returns null if no task list with the given ID exists.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<TaskList> GetTaskListByIdAsync(int id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var taskList = await context.Set<TaskList>().Where(t => t.Id == id).FirstOrDefaultAsync();

        return taskList;
    }
}