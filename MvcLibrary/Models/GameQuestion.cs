using System.ComponentModel.DataAnnotations;

namespace MvcLibrary.Models;

public class GameQuestion
{
    [Key] public int GameQuestionId { get; set; }

    [Required] public string? Question { get; set; }

    [Required] public string? Answer { get; set; }
}