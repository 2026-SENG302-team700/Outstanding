using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace SENG302.Api.Models.Entities;

public enum CurrentTaskStatus
{
    Todo,
    InProgress,
    Done
}

/// <summary>
/// Represents a task that a user can create.
/// TaskId is a variable set by the constructor when it's created, 
/// and should not be set after the case, so therefore it does not 
/// have a setter (and therefore, it cannot be set as required).
/// </summary>
[PrimaryKey(nameof(TaskId), nameof(TaskListId))]
public class TaskItem
{
    public int TaskId { get; init; }
    public required int TaskListId { get; set; }
    [MaxLength(128)]
    public required string Name { get; set; }
    [MaxLength(2048)]
    public required string Description { get; set; } //constructor gives Description the value "No Description." if nothing was entered
    public DateTime DueDate { get; set; }
    public CurrentTaskStatus CurrentStatus { get; set; } = CurrentTaskStatus.Todo;
}
