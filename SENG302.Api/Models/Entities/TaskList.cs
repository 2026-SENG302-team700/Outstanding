using System.ComponentModel.DataAnnotations;

namespace SENG302.Api.Models.Entities;

/// <summary>
/// Represents a list of tasks that a user can create. 
/// Each task list has a name and is associated with a user's email.
/// </summary>
public class TaskList
{
    [Key]
    public int Id { get; set; }

    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty; // Foreign key to the User that owns this task list

    public int NextId { get; set; } = 0; // When a task is created, it'll be assigned this Id and this variable will be incremented

}
