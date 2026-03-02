using Microsoft.Net.Http.Headers;

namespace SENG302.Api.Models.Entities;

/// <summary> 
/// Object used to store the email and the password entered in the frontend to be processed for login verification
/// </summary>
public class UserCredentials {
    public string Email { get; set; }
    public string PasswordKey {get; set;}
}