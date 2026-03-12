using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SENG302.Api.Models.Entities;

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

    [ForeignKey("CustomFile")] 
    public string ProfilePicture { get; set; } = String.Empty;

    public DateTimeOffset TimeCreated { get; set; }
}