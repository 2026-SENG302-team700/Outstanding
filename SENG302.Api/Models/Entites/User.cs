using System.ComponentModel.DataAnnotations;

namespace SENG302.Api.Models.Entities;

public class User
{
    [Key]
    public required string Email { get; set; }

    public required string DisplayName { get; set; }

    public required string PasswordKey { get; set; }

    public required string Country { get; set; }

    public DateTimeOffset TimeCreated { get; set; }
}