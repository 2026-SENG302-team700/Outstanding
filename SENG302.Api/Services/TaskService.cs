using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
namespace SENG302.Api.Services;

public interface ITaskService
{
    Task<TaskList> CreateNewTaskListAsync(string name, string userEmail);
    Task<TaskList> GetTaskListByIdAsync(int id);
    Task<IEnumerable<TaskList>> GetTaskListsByUserEmailAsync(string userEmail);
    Task<IEnumerable<TaskItem>> GetTaskItemsByListAsync(int taskListId);
    bool VerifyUserExists(DatabaseContext context, string userEmail);
    Task<TaskItem> CreateNewTaskItemAsync(NewTaskItemRequest taskItem);
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

        // Remove trailing whitespace
        name = name.Trim();

        // Validate name length
        if (string.IsNullOrEmpty(name) || name.Length < 3 || name.Length > 128)
        {
            throw new ArgumentException("List name is required and must be between 3 and 128 characters long");
        }
        // Validate name characters (only allow letters, numbers, spaces, hyphens, and apostrophes)
        if (!Regex.IsMatch(name, @"^[\p{L}0-9\s'-]+$"))
        {
            throw new ArgumentException("List name cannot contain characters other than letters, spaces, hyphens, apostrophes, or numbers");
        }

        var user = await context.Users.Where(u => u.Email == userEmail).FirstOrDefaultAsync();
        // Validate user email exists in db
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
        if (taskList == null)
        {
            return null;
        }

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

    /// <summary>
    /// Adds a new task to the task list given owned by the given user. All parameters
    /// must be present (except description, dueDate and currentStatus), otherwise it fails. 
    /// The name of the task must be between 3 and 128 characters, and the description
    /// can be up to 2048 characters.
    /// dueDate must be in the future, and currentStatus will be automatically set to
    /// ToDo if not present.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="taskListId"></param>
    /// <param name="name"></param>
    /// <param name="dueDate"></param>
    /// <param name="currentStatus"></param>
    /// <param name="description"></param>
    /// <returns>the newly created TaskItem</returns>
    public async Task<TaskItem> CreateNewTaskItemAsync(NewTaskItemRequest taskItem)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var list = await GetTaskListByIdAsync(taskItem.TaskListId, context);
        if (taskItem.DueDate.Date.ToString("dd/MM/yyyy") != "01/01/0001"){
            DateTime today = DateTime.Now;
            if (taskItem.DueDate < today) 
            {
                throw new ArgumentException("Invalid due date, date must be in the future");
            }

            context.TaskLists.Where(u => u.Id == list.Id)
                         .ExecuteUpdate(b => b.SetProperty(u => u.NextId, list.NextId += 1));
            await context.SaveChangesAsync();
            var newTask = new TaskItem()
            {
                TaskListId = taskItem.TaskListId,
                TaskId = list.NextId,
                Name = taskItem.Name,
                Description = taskItem.Description,
                CurrentStatus = taskItem.CurrentStatus,
                DueDate = taskItem.DueDate
            };
            context.Set<TaskItem>().Add(newTask);
            await context.SaveChangesAsync();
            return newTask;
        } 
        else {
            context.TaskLists.Where(u => u.Id == list.Id)
                         .ExecuteUpdate(b => b.SetProperty(u => u.NextId, list.NextId += 1));
            await context.SaveChangesAsync();
            var newTask = new TaskItem()
            {
                TaskListId = taskItem.TaskListId,
                TaskId = list.NextId,
                Name = taskItem.Name,
                Description = taskItem.Description,
                CurrentStatus = taskItem.CurrentStatus,
                //ommits the due date
            };
            context.Set<TaskItem>().Add(newTask);
            await context.SaveChangesAsync();
            return newTask;
        }


    }

    /// <summary>
    /// Gets all task items from the given list.
    /// </summary>
    /// <param name="taskListId"></param>
    /// <returns>a list of all tasks found. Empty if no tasks exist.</returns>
    public async Task<IEnumerable<TaskItem>> GetTaskItemsByListAsync(int taskListId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var taskItems = await context.Set<TaskItem>().Where(t => t.TaskListId == taskListId).ToListAsync();

        return taskItems;
    }

    /// <summary>
    /// Given this method is given a valid database context, query the context
    /// to verify if the given email is registered to a user in the db.
    /// The email is used as the user's primary key, and is therefore, unique.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="userEmail"></param>
    /// <returns>true if email exists, false otherwise</returns>
    public bool VerifyUserExists(DatabaseContext context, string userEmail)
    {
        return context.Users.Where(u => u.Email == userEmail).FirstOrDefaultAsync() != null;

    }
}

