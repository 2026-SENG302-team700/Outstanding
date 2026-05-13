using System.ComponentModel.DataAnnotations;
using SENG302.Api.Models.Entities;

namespace SENG302.Api.Models.Interfaces;

public interface ITaskItemRequest {
    [MaxLength(128)]
    public  string Name { get; set; }

    [MaxLength(2048)]
    public  string Description { get; set; }

    public  DateTime? DueDate { get; set; }

    public CurrentTaskStatus CurrentStatus { get; set; }
}