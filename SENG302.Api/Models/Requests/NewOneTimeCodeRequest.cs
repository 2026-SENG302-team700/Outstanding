namespace SENG302.Api.Models.Requests;

public class NewOneTimeCodeRequest
{
    public required string Email { get; set; } = String.Empty;
    public bool ResendingCode { get; set; } = false;
}