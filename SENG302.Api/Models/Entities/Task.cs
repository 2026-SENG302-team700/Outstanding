using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SENG302.Api.Models.Entities;

public enum CurrentTaskStatus {
    Todo,
    InProgress,
    Done
}

/// <summary>
/// Represents a task that a user can create 
/// TaskId and Owner Email are variables set by the constructor when it's 
/// created, and should not be set after the case, so therefore it does not 
/// have a setter (and therefore, it cannot be set as required).
/// </summary>
public class TaskItem
{
    [Key]
    public int TaskId {get;}
    public string OwnerEmail {get;}
    public required int TaskListId {get; set;}
    
    [MaxLength(128)]
    public required string Name {get; set;}

    [MaxLength(2048)]
    public string Description {get; set;} = "No Description.";

    [AllowNull]
    public DateTime DueDate {get; set;}

    public CurrentTaskStatus CurrentStatus {get; set;} = CurrentTaskStatus.Todo;

}
