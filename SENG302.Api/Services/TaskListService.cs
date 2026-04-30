using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
namespace SENG302.Api.Services;

public interface ITaskListService
{
    Task<TaskList> CreateNewTaskListAsync(string name, int userId);
    Task<TaskList> GetTaskListByIdAsync(int id, int userId);
    Task<IEnumerable<TaskList>> GetTaskListsByUserIdAsync(int userId);
}

public class TaskListService : ITaskListService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly TimeProvider _timeProvider;

    public TaskListService(IDbContextFactory<DatabaseContext> dbContextFactory, TimeProvider timeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Gets all task lists associated with a user's email and checks for profanity if applicable
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TaskList>> GetTaskListsByUserIdAsync(int userId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var taskLists = await context.Set<TaskList>().Where(t => t.UserId == userId).ToListAsync();

        var user = await context.Users.FindAsync(userId);
        // check for profanity filtering
        if (user?.ProfanityFiltering != true) return taskLists;

        var profanityFiltering = new ProfanityFilter.ProfanityFilter();
        foreach (TaskList taskList in taskLists)
        {
            taskList.Name = profanityFiltering.CensorString(taskList.Name);
        }
        return taskLists;
    }

    /// <summary>
    /// Creates a new task list for a user with the given id and name. Validates the 
    /// name and user id before creating the task list.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<TaskList> CreateNewTaskListAsync(string name, int userId)
    {
        // Get a database context
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        // Remove trailing whitespace
        name = name.Trim();

        // Validate name length
        if (string.IsNullOrEmpty(name) || name.Length < 3 || name.Length > 128)
        {
            throw new ArgumentException("List name is required and must be between 3 and 128 characters long");
        }
        // Validate name characters (only allow letters, numbers, spaces, hyphens, and apostrophes)
        if (!ValidationPatterns.TaskListName.IsMatch(name))
        {
            throw new ArgumentException("List name cannot contain characters other than letters, spaces, hyphens, apostrophes, or numbers");
        }

        var user = await context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
        // Validate user email exists in db
        if (user == null)
        {
            throw new ArgumentException("User with the provided id does not exist.");
        }

        // Create the new task list
        var newTaskList = new TaskList()
        {
            Name = name,
            UserId = userId
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
    public async Task<TaskList> GetTaskListByIdAsync(int id, int userId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var taskList = await context.Set<TaskList>().Where(t => t.Id == id).FirstOrDefaultAsync();
        if (taskList == null)
        {
            return null;
        }
        var user = await context.Users.FindAsync(userId);
        // check for profanity censor
        if (user?.ProfanityFiltering != true) return taskList;

        var profanityFilter = new ProfanityFilter.ProfanityFilter();
        taskList.Name = profanityFilter.CensorString(taskList.Name);
        return taskList;
    }

    /// <summary>
    /// Gets a task list by its ID. Returns null if no task list with the given ID exists.
    /// Also takes a DatabaseContext so the list that it returns can be modified.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<TaskList> GetTaskListByIdAsync(int id, DatabaseContext context)
    {
        var taskList = await context.Set<TaskList>().Where(t => t.Id == id).FirstOrDefaultAsync();
        if (taskList == null)
        {
            throw new ArgumentException("Task list with the provided ID does not exist.");
        }

        return taskList;
    }
}

