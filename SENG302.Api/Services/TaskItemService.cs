using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
namespace SENG302.Api.Services;

public interface ITaskItemService
{
    Task<IEnumerable<TaskItem>> GetTaskItemsByListAsync(int taskListId);
    Task<TaskItem> CreateNewTaskItemAsync(NewTaskItemRequest taskItem);
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

        DateTime creationTime = DateTime.Now;
        if (taskItem.DueDate < creationTime)
        {
            throw new ArgumentException("Invalid due date, date must be in the future");
        }

        if (taskItem.Description == "")
        {
            taskItem.Description = "No Description";
        }
        var newTask = new TaskItem()
        {
            TaskListId = taskItem.TaskListId,
            Name = taskItem.Name,
            Description = taskItem.Description,
            CurrentStatus = taskItem.CurrentStatus,
            DueDate = taskItem.DueDate
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

}

