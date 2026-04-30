namespace SENG302.Api.Models.Requests;

public class UpdateUserRequest
{
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;
    
    public bool ProfanityFiltering { get; set; }
}