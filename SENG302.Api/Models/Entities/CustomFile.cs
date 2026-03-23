using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SENG302.Api.Models.Entities;

public class CustomFile
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("User")]
    public required int OwnerId { get; set; }
    
    public required string FileKey { get; set; }
    
    public required string OriginalFileName { get; set; }
    
    public required string MimeType { get; set; }

    public long FileSize { get; set; }
}