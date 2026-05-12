using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Interfaces;
using SENG302.Api.Resources.Helpers;

public class TaskItemValidator {

    /// <summary>
    /// Checks the due date is valid
    /// adds error to dictionary when error occurs.
    /// </summary>
    /// <param name="dueDate"></param>
    public static Dictionary<string, string> ValidateTaskItemDueDate(DateTime? dueDate)
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
    public static void ValidateTaskItemCurrentStatus(CurrentTaskStatus currentStatus)
    {
        if (!Enum.IsDefined(typeof(CurrentTaskStatus), currentStatus))
        {
            throw new ArgumentOutOfRangeException(
                "currentStatus",
                "Status invalid, refresh your browser (or internal server error)"
                );
        }
    }

    public static Dictionary<string, string> ValidateTaskItemFields(ITaskItemRequest taskItem, bool profanityFiltering) {
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

        return errors;
    }


    /// <summary>
    /// Checks the task item name is valid, returns nothing if valid
    /// adds error to dictionary when error occurs.
    /// </summary>
    /// <param name="name">The name being tested</param>
    public static Dictionary<string, string> ValidateTaskItemName(string name, bool profanityFiltering)
    {
        var errors = new Dictionary<string, string>();
        var taskItemName = name.Trim();
        if (taskItemName.Length < 3 || taskItemName.Length > 128)
        {
            errors["name"] = "Title is required and must be between 3 and 128 characters long";
        }
        if (ProfanityTools.ContainsProfanity(name, profanityFiltering)) errors["name"] = "Title cannot contain profanity.";

        return errors;
    }

    /// <summary>
    /// Checks the description is valid
    /// adds error to dictionary when error occurs.
    /// </summary>
    /// <param name="description">The description being tested</param>
    public static Dictionary<string, string> ValidateTaskItemDescription(string description)
    {
        var errors = new Dictionary<string, string>();
        if (description.Trim().Length > 2048)
        {
            errors["description"] = "Description must be 2048 characters or less";
        }

        return errors;
    }
}