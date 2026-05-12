using System.ComponentModel.DataAnnotations;
using SENG302.Api.Models.Interfaces;

namespace SENG302.Api.Models.Entities;

/// <summary>
/// Represents a task that a user can create.
/// TaskId is a variable set by the constructor when it's created, 
/// and should not be set after the case, so therefore it does not 
/// have a setter (and therefore, it cannot be set as required).
/// </summary>
public class NewTaskItemRequest : ITaskItemRequest
{
    public required int TaskListId { get; set; }

    [MaxLength(128)]
    public required string Name { get; set; }

    [MaxLength(2048)]
    public required string Description { get; set; }

    public required DateTime? DueDate { get; set; }

    public CurrentTaskStatus CurrentStatus { get; set; } = CurrentTaskStatus.Todo;
}
