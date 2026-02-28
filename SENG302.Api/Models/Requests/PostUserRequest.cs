using System.ComponentModel.DataAnnotations;
using SENG302.Api.Models.Entities;

namespace SENG302.Api.Models.Requests;

public class PostUserRequest
{
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    
    public string PasswordKey { get; set; } = string.Empty;

    public string PasswordConfirm { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;
}