using Microsoft.Net.Http.Headers;

namespace SENG302.Api.Models.Entities;

public class UserCredentials {
    public string Email { get; set; }
    public string PasswordKey {get; set;}
}