using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcLibrary.Models;

public class CartItem
{
    [Key] public int CartItemId { get; set; }

    [ForeignKey("BookId")] public Book? Book { get; set; }

    [ForeignKey("CartId")] public Cart? Cart { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
    public int Quantity { get; set; }
}