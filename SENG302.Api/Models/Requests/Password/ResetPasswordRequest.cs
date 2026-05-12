namespace SENG302.Api.Models.Requests.Password;

/// <summary>
/// A class representing a request to update the users password
/// The request requires the users old password and the new one 
/// repeated twice
/// </summary>
public class ResetPasswordRequest
{
    public required string NewPassword { get; set; }
    public required string NewPasswordConfirm { get; set; }
}