namespace SENG302.Api.Models.Requests;

public class ValidateOneTimeCodeRequest
{
    public required string Email { get; set; } = String.Empty;
    
    public required string Code { get; set; }

    public bool TimeLimitExists { get; set; } = true;
}