using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using SENG302.Api.Resources.Helpers;
using SENG302.Api.Models.Requests;


namespace SENG302.Api.Services;

public interface ITaskItemService
{
    Task<IEnumerable<TaskItem>> GetAllTaskItemsAsync(int userId);
    Task<IEnumerable<TaskItem>> GetTaskItemsByListAsync(int taskListId);
    Task<TaskItem> CreateNewTaskItemAsync(NewTaskItemRequest taskItem, int userId);
    Task<TaskItem?> GetTaskItemAsync(int id, int userId);
    Task<TaskItem> UpdateTaskItemAsync(UpdateTaskItemRequest taskItemUpdates, int userId);
}


public class TaskItemService : ITaskItemService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ProfanityTools _profanityTools;

    public TaskItemService(IDbContextFactory<DatabaseContext> dbContextFactory, TimeProvider timeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Checks the task item name is valid, returns nothing if valid
    /// adds error to dictionary when error occurs.
    /// </summary>
    /// <param name="name">The name being tested</param>
    public Dictionary<string, string> ValidateTaskItemName(string name, bool profanityFiltering)
    {
        var errors = new Dictionary<string, string>();
        var taskItemName = name.Trim();
        if (taskItemName.Length < 3 || taskItemName.Length > 128)
        {
            errors["name"] = "Title is required and must be between 3 and 128 characters long";
        }



        if (_profanityTools.ContainsProfanity(name, profanityFiltering)) errors["name"] = "Title cannot contain profanity.";

        return errors;
    }

    /// <summary>
    /// Checks the description is valid
    /// adds error to dictionary when error occurs.
    /// </summary>
    /// <param name="description">The description being tested</param>
    public Dictionary<string, string> ValidateTaskItemDescription(string description)
    {
        var errors = new Dictionary<string, string>();
        if (description.Trim().Length > 2048)
        {
            errors["description"] = "Description must be 2048 characters or less";
        }

        return errors;
    }

    /// <summary>
    /// Get all the tasks from all the lists
    /// </summary>
    /// <returns> a list of all the tasks </returns>
    public async Task<IEnumerable<TaskItem>> GetAllTaskItemsAsync(int userId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Set<TaskItem>()
        .Where(t => context.Set<TaskList>()
            .Any(l => l.Id == t.TaskListId && l.UserId == userId))
        .ToListAsync();
    }

    /// <summary>
    /// Checks the due date is valid
    /// adds error to dictionary when error occurs.
    /// </summary>
    /// <param name="dueDate"></param>
    public Dictionary<string, string> ValidateTaskItemDueDate(DateTime? dueDate)
    {
        var errors = new Dictionary<string, string>();
        DateTime currentTime = DateTime.UtcNow;

        if (dueDate != null && currentTime > dueDate)
        {
            errors["dueDate"] = "Invalid due date, date must be in the future";
        }

        return errors;
    }

    /// <summary>
    /// Checks to see if task item's current status is valid.
    /// </summary>
    /// <param name="currentStatus">the status of the task item</param>
    /// <exception cref="ArgumentOutOfRangeException">if not valid status (shouldn't occur naturally)</exception>
    public void ValidateTaskItemCurrentStatus(CurrentTaskStatus currentStatus)
    {
        if (!Enum.IsDefined(typeof(CurrentTaskStatus), currentStatus))
        {
            throw new ArgumentOutOfRangeException(
                "currentStatus",
                "Status invalid, refresh your browser (or internal server error)"
                );
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
    public async Task<TaskItem> CreateNewTaskItemAsync(NewTaskItemRequest taskItem, int userId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FindAsync(userId);
        var profanityFiltering = user?.ProfanityFiltering ?? false;

        // Clean request
        taskItem.Name = taskItem.Name.Trim();
        taskItem.Description = taskItem.Description.Trim();
        taskItem.DueDate = taskItem.DueDate == DateTime.MinValue ? null : taskItem.DueDate;

        // Validation
        var errors = new Dictionary<string, string>();

        foreach (var (key, value) in ValidateTaskItemName(taskItem.Name, profanityFiltering))
        {
            errors.Add(key, value);
        }

        foreach (var (key, value) in ValidateTaskItemDescription(taskItem.Description))
        {
            errors.Add(key, value);
        }

        foreach (var (key, value) in ValidateTaskItemDueDate(taskItem.DueDate))
        {
            errors.Add(key, value);
        }

        ValidateTaskItemCurrentStatus(taskItem.CurrentStatus); // should not occur naturally, therefore handled differently.

        if (errors.Count > 0)
        {


        }

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

    public async Task<TaskItem?> GetTaskItemAsync(int id, int userId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FindAsync(userId);
        var profanityFiltering = user?.ProfanityFiltering ?? false;

        var taskItem = await context.TaskItems.FirstOrDefaultAsync(t => t.TaskId == id);

        if (!profanityFiltering) return taskItem;

        var profanityFilter = new ProfanityFilter.ProfanityFilter();
        var censoredName = profanityFilter.CensorString(taskItem.Name);
        var censoredDescription = profanityFilter.CensorString(taskItem.Description);

        taskItem.Name = censoredName;
        taskItem.Description = censoredDescription;

        return taskItem;
    }

    /// <summary>
    /// Brings in an update task item request from the controller
    /// Strips the name and modifies the due date to be valid
    /// Validates incoming fields
    /// Updates the content then pushes to DB.
    /// </summary>
    /// <param name="taskItemUpdates">Incoming Task Item Request</param>
    /// <returns>The updated task item</returns>
    public async Task<TaskItem> UpdateTaskItemAsync(UpdateTaskItemRequest taskItemUpdates, int userId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FindAsync(userId);
        var profanityFiltering = user?.ProfanityFiltering ?? false;

        var taskItem = await context.TaskItems.FirstOrDefaultAsync(u => u.TaskId == taskItemUpdates.taskId);
        if (taskItem == null)
        {
            return null;
        }

        taskItemUpdates.Name = taskItemUpdates.Name.Trim();
        taskItemUpdates.Description = taskItemUpdates.Description.Trim();
        taskItemUpdates.DueDate = (taskItemUpdates.DueDate == DateTime.MinValue) ? null : taskItemUpdates.DueDate;

        var errors = new Dictionary<string, string>();

        foreach (var (key, value) in ValidateTaskItemName(taskItemUpdates.Name, profanityFiltering))
        {
            errors[key] = value;
        }

        foreach (var (key, value) in ValidateTaskItemDescription(taskItemUpdates.Description))
        {
            errors[key] = value;
        }

        foreach (var (key, value) in ValidateTaskItemDueDate(taskItemUpdates.DueDate))
        {
            errors[key] = value;
        }

        ValidateTaskItemCurrentStatus(taskItemUpdates.CurrentStatus); // should not occur naturally, therefore handled differently.

        if (errors.Count > 0)
        {
            throw new MultipleValidationException(errors);
        }

        taskItem.Name = taskItemUpdates.Name;
        taskItem.Description = taskItemUpdates.Description;
        taskItemUpdates.DueDate = taskItemUpdates.DueDate == DateTime.MinValue ? null : taskItemUpdates.DueDate;
        taskItem.DueDate = taskItemUpdates.DueDate;
        taskItem.CurrentStatus = taskItemUpdates.CurrentStatus;

        context.TaskItems.Update(taskItem);
        await context.SaveChangesAsync();
        return taskItem;
    }
}

