using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MvcLibrary.Models;

public class Cart
{
    private decimal _cartTotalPrice;

    private decimal _promoDiscount;
    
    
    
    [Key] public int CartId { get; set; }

    [ForeignKey("UserId")] public IdentityUser? User { get; set; }

    public List<CartItem>? CartItems { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Cart total price must be a non-negative value")]
    public decimal CartTotalPrice
    {
        get => _cartTotalPrice;
        set => _cartTotalPrice = Math.Round(value, 1);
    }

    [Range(0, int.MaxValue, ErrorMessage = "Cart total items must be a non-negative value")]
    public int CartTotalItems { get; set; }

    [Range(0, 1, ErrorMessage = "Discount must be between 0 and 1")]
    public decimal PromoDiscount
    {
        get => _promoDiscount;
        set => _promoDiscount = Math.Round(value, 2);
    }

    public bool PromoApplied { get; set; }

    [ForeignKey("PromoId")] public Promo? Promo { get; set; }
}