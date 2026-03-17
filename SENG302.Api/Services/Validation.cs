using System.Text.Json;
using System.Text.RegularExpressions;

namespace SENG302.Api.Services;

public class RegexConfig
{
    public required UserPatterns User { get; set; }
    public required TaskListPatterns TaskList { get; set; }
    public required TaskItemPatterns TaskItem { get; set; }
}

public class UserPatterns
{
    public required string Email { get; set; }
    public required DisplayNamePattern DisplayName { get; set; }
    public required string Password { get; set; }
}

public class DisplayNamePattern
{
    public required string Pattern { get; set; }
    public required string Flags { get; set; }
}

public class TaskListPatterns
{
    public required ListNamePattern Name { get; set; }
}

public class ListNamePattern
{
    public required string Pattern { get; set; }
    public required string Flags { get; set; }
}

public class TaskItemPatterns
{

}


/// <summary>
/// Provides a list of regex patterns read from a shared json file
/// </summary>
public static class ValidationPatterns
{
    // User Patterns
    public static Regex UserEmail { get; private set; }
    public static Regex UserDisplayName { get; private set; }
    public static Regex UserPassword { get; private set; }

    // Task List Patterns
    public static Regex TaskListName { get; private set; }

    /// <summary>
    /// Returns a set of RegexOptions based on the given flags
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public static RegexOptions setDisplayOptions(string flags)
    {
        var displayOptions = RegexOptions.Compiled;
        if (flags.Contains("u") == true)
            displayOptions |= RegexOptions.CultureInvariant;
        return displayOptions;
    }
    static ValidationPatterns()
    {
        try
        {
            var jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "regexPatterns.json");
            var fullPath = Path.GetFullPath(jsonFilePath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Regex config file not found.", fullPath);

            var json = File.ReadAllText(fullPath);
            var config = JsonSerializer.Deserialize<RegexConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Failed to deserialize regex config.");



            // Set User Patterns
            UserEmail = new Regex(config.User.Email, RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));




            UserDisplayName = new Regex(config.User.DisplayName.Pattern, setDisplayOptions(config.User.DisplayName.Flags), TimeSpan.FromMilliseconds(200));
            UserPassword = new Regex(config.User.Password, RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

            // Set tasklist patterns
            TaskListName = new Regex(config.TaskList.Name.Pattern, RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));
        }
        catch (Exception ex)
        {
            // Rethrow with extra info so you can see the root cause
            throw new Exception("ValidationPatterns static constructor failed.", ex);
        }
    }
}