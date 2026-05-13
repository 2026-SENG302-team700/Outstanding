namespace SENG302.Api.Models.Requests;

public class BadRequestValidationResponse
{
    public Dictionary<string, string> Errors { get; set; } = new();
}