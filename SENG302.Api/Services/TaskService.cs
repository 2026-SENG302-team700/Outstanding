using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SENG302.Api.Services;

public interface ITaskService
{
    Task<TaskList> GenerateNewTaskListAsync(string name, string userEmail);
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

    public async Task<TaskList> GenerateNewTaskListAsync(string name, string userEmail)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.Where(u => u.Email == userEmail).FirstOrDefaultAsync();

        var taskList = new TaskList
        {
            Name = name
        };

        return taskList;
    }
}