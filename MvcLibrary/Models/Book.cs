using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Range = Microsoft.CodeAnalysis.Elfie.Model.Structures.Range;

namespace MvcLibrary.Models;

public class Book
{
    public int Id { get; set; }

    [Required] 
    public string? Name { get; set; }

    [Required] 
    [RegularExpression("^[A-Za-z][A-Za-z ]*$", ErrorMessage = "Invalid Author format. Author cannot start with a number or special characters and can contain only letters.")]
    public string? Author { get; set; }

    [Required] public string? Description { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Price can't be negative")]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stock can't be negative")]
    public int Stock { get; set; }

    [NotMapped] [Required] public IFormFile? Image { get; set; }

    public string? ImageUrl { get; set; }

    
    private decimal _cumulativeRating;

    [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
    public decimal CumulativeRating
    {
        get => _cumulativeRating;
        set => _cumulativeRating = Math.Round(value, 1);
    }

    public List<Review>? BookReviews { get; set; }
}