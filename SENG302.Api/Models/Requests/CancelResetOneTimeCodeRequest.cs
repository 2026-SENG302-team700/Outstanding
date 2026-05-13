namespace SENG302.Api.Models.Requests;

public class CancelResetOneTimeCodeRequest
{
    public required string Email { get; set; } = String.Empty;
}