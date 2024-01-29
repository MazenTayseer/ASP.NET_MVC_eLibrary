using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MvcLibrary.Models;

public class Review
{
    [Key] public int ReviewId { get; set; }

    [ForeignKey("UserId")] public IdentityUser? User { get; set; }

    [ForeignKey("BookId")] public Book? Book { get; set; }
    
    [Required]
    [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
    public int Rating { get; set; }

    [Required] public string? Comment { get; set; }
}