using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SENG302.Api.Models.Entities;


[Index(nameof(Email), IsUnique = true)]
public class User
{
    [Key]
    public int Id { get; set; }
    public required string Email { get; set; }

    [MinLength(3)]
    [MaxLength(64)]
    public required string DisplayName { get; set; }

    public string PasswordKey { get; set; } = String.Empty;

    public required string Country { get; set; }

    public int ProfilePicture { get; set; }

    public DateTimeOffset TimeCreated { get; set; }

    public bool EmailVerified { get; set; } = true;
}