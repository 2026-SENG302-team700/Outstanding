using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Globalization;
namespace SENG302.Api.Services;

public interface ITaskItemService
{
    Task<IEnumerable<TaskItem>> GetTaskItemsByListAsync(int taskListId);
    Task<TaskItem> CreateNewTaskItemAsync(NewTaskItemRequest taskItem);
    Task<TaskItem?> GetTaskItemAsync(int id);
}


public class TaskItemService : ITaskItemService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly TimeProvider _timeProvider;

    public TaskItemService(IDbContextFactory<DatabaseContext> dbContextFactory, TimeProvider timeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Checks the task item name is valid, returns nothing if valid
    /// but throws error if invalid
    /// </summary>
    /// <param name="name">The name being tested</param>
    /// <exception cref="InvalidLengthException"></exception>
    public void ValidateTaskItemName(string name)
    {
        if (name.Length < 3 || name.Length > 128)
        {
            throw new InvalidLengthException("Title is required and must be between 3 and 128 characters long");
        }
    }

    /// <summary>
    /// Checks the description is valid 
    /// </summary>
    /// <param name="description">The description being tested</param>
    /// <exception cref="InvalidLengthException"></exception>
    public void ValidateTaskItemDescription(string description)
    {
        if (description.Length > 2048)
        {
            throw new InvalidLengthException("Description must be 2048 characters or less");
        }
    }

    /// <summary>
    /// Checks the due date is valid
    /// </summary>
    /// <param name="dueDate"></param>
    /// <exception cref="ArgumentException"></exception>
    public void ValidateTaskItemDueDate(DateTime? dueDate)
    {
        DateTime currentTime = DateTime.UtcNow;
        if (dueDate != null && currentTime > dueDate)
        {
            throw new ArgumentException("Invalid due date, date must be in the future");
        }
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

        // Clean request
        taskItem.Name = taskItem.Name.Trim();
        taskItem.DueDate = taskItem.DueDate == DateTime.MinValue ? null : taskItem.DueDate;

        // Validation
        ValidateTaskItemName(taskItem.Name);
        ValidateTaskItemDescription(taskItem.Description);
        ValidateTaskItemDueDate(taskItem.DueDate);

        // Set default descriptiom
        if (taskItem.Description == "") taskItem.Description = "No Description";

        // Add task item
        var newTask = new TaskItem()
        {
            TaskListId = taskItem.TaskListId,
            Name = taskItem.Name,
            Description = taskItem.Description,
            CurrentStatus = taskItem.CurrentStatus,
            DueDate = taskItem.DueDate,
            creationTime = DateTime.UtcNow
        };
        context.Set<TaskItem>().Add(newTask);
        await context.SaveChangesAsync();
        return newTask;
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

    public async Task<TaskItem?> GetTaskItemAsync(int id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var taskItem = await context.TaskItems.FirstOrDefaultAsync(t => t.TaskId == id);
        return taskItem;
    }

}

