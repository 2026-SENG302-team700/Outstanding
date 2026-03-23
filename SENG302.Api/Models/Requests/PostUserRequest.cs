namespace SENG302.Api.Models.Requests;

public class PostUserRequest
{
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    
    public string PasswordString { get; set; } = string.Empty;

    public string PasswordConfirm { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;
}