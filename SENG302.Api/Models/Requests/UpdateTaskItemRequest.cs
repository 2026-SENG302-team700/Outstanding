using System.ComponentModel.DataAnnotations;


namespace SENG302.Api.Models.Entities;

/// <summary>
/// Represents updates to a pre-existing task.
/// TaskId needs to be present, everything else optional.
/// </summary>
public class UpdateTaskItemRequest
{
    public required int taskId { get; set; }

    [MaxLength(128)]
    public required string Name { get; set; }

    [MaxLength(2048)] public string Description { get; set; } = string.Empty;

    public required DateTime? DueDate { get; set; }

    public CurrentTaskStatus CurrentStatus { get; set; } = CurrentTaskStatus.Todo;
}
