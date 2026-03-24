using System.ComponentModel.DataAnnotations;
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

    public string PasswordKey { get; set; } = string.Empty;

    public required string Country { get; set; }

    public int ProfilePicture { get; set; }

    public float ProfilePictureOffsetX { get; set; } = 0;

    public float ProfilePictureOffsetY { get; set; } = 0;

    public float ProfilePictureZoom { get; set; } = 1;

    [StringLength(6)]
    public string OneTimeCode { get; set; } = string.Empty;

    public long CodeGenerationTime { get; set; }

    public DateTimeOffset TimeCreated { get; set; }

    public bool EmailVerified { get; set; } = false;
}