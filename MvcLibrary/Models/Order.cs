using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MvcLibrary.Models;

public class Order
{
    private decimal _orderTotalPrice;
    [Key] public int OrderId { get; set; }

    [ForeignKey("UserId")] public IdentityUser? User { get; set; }

    [Required(ErrorMessage = "First name is required")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Address is required")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Country is required")]
    public string? Country { get; set; }

    [Required(ErrorMessage = "City is required")]
    public string? City { get; set; }

    [Required(ErrorMessage = "Zip code is required")]
    [RegularExpression(@"^\d{1,5}$", ErrorMessage = "Zip code must be numeric and have at most 5 digits")]
    [MaxLength(5, ErrorMessage = "Zip code must be at most 5 digits")]
    public string? ZipCode { get; set; }

    public List<OrderItem>? OrderItems { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Order total price must be a non-negative value")]
    public decimal OrderTotalPrice
    {
        get => _orderTotalPrice;
        set => _orderTotalPrice = Math.Round(value, 2);
    }

    [Range(0, int.MaxValue, ErrorMessage = "Order total items must be a non-negative value")]
    public int OrderTotalItems { get; set; }

    public string? PayPalDetails { get; set; }
}