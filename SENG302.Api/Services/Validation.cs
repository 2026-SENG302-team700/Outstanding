using System.Text.Json;
using System.Text.RegularExpressions;

namespace SENG302.Api.Services;

public class RegexConfig
{
    public required UserPatterns User { get; set; }
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


public static class ValidationPatterns
{
    public static Regex Email { get; private set; }
    public static Regex DisplayName { get; private set; }
    public static Regex Password { get; private set; }

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

            Email = new Regex(config.User.Email, RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

            var displayOptions = RegexOptions.Compiled;
            if (config.User.DisplayName.Flags?.Contains("u") == true)
                displayOptions |= RegexOptions.CultureInvariant;

            DisplayName = new Regex(config.User.DisplayName.Pattern, displayOptions, TimeSpan.FromMilliseconds(200));
            Password = new Regex(config.User.Password, RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));
        }
        catch (Exception ex)
        {
            // Rethrow with extra info so you can see the root cause
            throw new Exception("ValidationPatterns static constructor failed.", ex);
        }
    }
}