using System.ComponentModel.DataAnnotations;

namespace SENG302Template.Api.Models.Entities;

public class Book
{
  [Key]
  public int Id { get; set; }

  [MaxLength(50)]
  public string Title { get; set; } = string.Empty;

  [MaxLength(200)]
  public string Author { get; set; } = string.Empty;

  public int Year { get; set; }

  public DateTimeOffset Created { get; set; }
}
