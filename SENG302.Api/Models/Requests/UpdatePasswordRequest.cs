using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace SENG302.Api.Models.Entities;

/// <summary>
/// A class representing a request to update the users password
/// The request requires the users old password and the new one 
/// repeated twice
/// </summary>
public class UpdatePasswordRequest
{
    public string OldPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string NewPasswordConfirm { get; set; }
}
