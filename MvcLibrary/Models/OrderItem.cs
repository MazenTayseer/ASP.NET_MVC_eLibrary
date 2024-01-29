using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcLibrary.Models;

public class OrderItem
{
    [Key] public int OrderItemId { get; set; }

    [ForeignKey("BookId")] public Book? Book { get; set; }

    [ForeignKey("OrderId")] public Order? Order { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
    public int Quantity { get; set; }
}