using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcLibrary.Data;

namespace MvcLibrary.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        var cart = await _context.Cart
            .Include(c => c.CartItems!)
            .ThenInclude(ci => ci.Book)
            .FirstOrDefaultAsync(c => c.User!.Id == user!.Id);

        ViewBag.CartItems = cart!.CartItems!.ToList();

        ViewData["Cart Total Items"] = cart.CartTotalItems;

        if (cart.PromoApplied)
            ViewData["Cart Total Price"] = Math.Round(cart.CartTotalPrice * (1 - cart.PromoDiscount), 2);
        else
            ViewData["Cart Total Price"] = Math.Round(cart.CartTotalPrice, 2);

        var promoUser = await _context.Promo.FirstOrDefaultAsync(p => p.User == user);

        if (promoUser != null)
        {
            var availablePromoUserCodes =
                (from promo in _context.Promo where promo.User == user select promo).ToList();
            ViewBag.AvailablePromoCodes = availablePromoUserCodes;
        }
        // else
        // {
        //     ViewBag.AvailablePromoCodes = "None";
        // }

        if (cart!.PromoApplied)
        {
            ViewBag.PromoApplied = true;
            var promoUserCode = await _context.Promo.FirstOrDefaultAsync(p => p.User == user);
            ViewData["PromoCode"] = promoUserCode!.Code;
        }
        else
        {
            ViewBag.PromoApplied = false;
            ViewData["PromoCode"] = "None";
        }

        if (TempData.ContainsKey("ErrorMessage")) ViewBag.ErrorMessage = TempData["ErrorMessage"]!.ToString()!;

        return View();
    }

    // Adjust Price and Total Items after deleting
    [HttpPost]
    public async Task<IActionResult> AdjustQuantity(int bookId, string action)
    {
        var user = await _userManager.GetUserAsync(User);

        var cart = await _context.Cart
            .Include(c => c.CartItems!)
            .ThenInclude(ci => ci.Book)
            .FirstOrDefaultAsync(c => c.User!.Id == user!.Id);

        var cartItem = cart!.CartItems!.FirstOrDefault(ci => ci.Book != null && ci.Book.Id == bookId);

        decimal newPrice = 0;

        if (cartItem != null)
        {
            if (action == "add")
            {
                cartItem.Quantity++;
            }
            else if (action == "remove")
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                }
                else
                {
                    cart.CartItems!.Remove(cartItem);
                    _context.CartItem.Remove(cartItem);


                    cart.CartTotalPrice = cart.CartItems.Sum(item => item.Book!.Price * item.Quantity);

                    newPrice = Math.Round(cart.CartTotalPrice, 2);
                    if (cart!.PromoApplied) newPrice = Math.Round(cart.CartTotalPrice * (1 - cart.PromoDiscount), 2);


                    // cart.CartTotalPrice = Math.Round(cart.CartTotalPrice * (1 - cart.PromoDiscount), 2);

                    cart.CartTotalItems = cart.CartItems.Sum(item => item.Quantity);
                    await _context.SaveChangesAsync();

                    return Json(new
                        { newQuantity = -1, newTotalPrice = newPrice, newTotalItems = cart.CartTotalItems });
                }
            }

            cart.CartTotalPrice = cart.CartItems!.Sum(item => item.Book!.Price * item.Quantity);

            newPrice = Math.Round(cart.CartTotalPrice, 2);
            if (cart!.PromoApplied) newPrice = Math.Round(cart.CartTotalPrice * (1 - cart.PromoDiscount), 2);


            cart.CartTotalItems = cart.CartItems!.Sum(item => item.Quantity);
            await _context.SaveChangesAsync();
        }

        return Json(new
        {
            newQuantity = cartItem!.Quantity, newTotalPrice = newPrice, newTotalItems = cart.CartTotalItems
        });
    }

    [HttpPost]
    public async Task<IActionResult> ApplyPromo(string promoCode)
    {
        var user = await _userManager.GetUserAsync(User);

        var cart = await _context.Cart.FirstOrDefaultAsync(c => c.User!.Id == user!.Id);

        var databasePromo = await _context.Promo.Include(u => u.User).FirstOrDefaultAsync(p => p.Code == promoCode);

        Console.WriteLine(databasePromo);
        if (databasePromo != null)
        {
            if (databasePromo.User == user)
            {
                if (!databasePromo.Used)
                {
                    if (cart!.PromoApplied == false)
                    {
                        cart.PromoApplied = true;
                        cart.PromoDiscount = (decimal)0.15;

                        cart.Promo = databasePromo;
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    return Json(new { message = "Fail", reason = "Promo Code is already used" });
                }
            }
            else
            {
                return Json(new { message = "Fail", reason = "This is not your Promo Code" });
            }
        }
        else
        {
            return Json(new { message = "Fail", reason = "Wrong Promo Code" });
        }

        return Json(new { message = "Success" });

        // var promoUser = await _context.Promo.FirstOrDefaultAsync(p => p.User == user);
        //
        // if (promoUser != null)
        // {
        //     var promoOfLoggedInUser = promoUser.Code;
        //
        //     Console.WriteLine("Mine = " + promoCode);
        //     Console.WriteLine("Database = " + promoOfLoggedInUser);
        //
        //     if (promoOfLoggedInUser == promoCode)
        //     {
        //         if (cart!.PromoApplied == false)
        //         {
        //             cart.PromoApplied = true;
        //             cart.PromoDiscount = (decimal)0.15;
        //             
        //             
        //             // cart.PromoId = promoCode;
        //             await _context.SaveChangesAsync();
        //         }
        //     }
        //     else
        //     {
        //         return Json(new { message = "Fail", reason = "Wrong Promo Code" });
        //     }
        // }
        //
        // return Json(new { message = "Success" });
    }

    [HttpPost]
    public async Task<IActionResult> RemovePromo()
    {
        var user = await _userManager.GetUserAsync(User);

        var cart = await _context.Cart.Include(p => p.Promo).FirstOrDefaultAsync(c => c.User!.Id == user!.Id);

        cart!.PromoApplied = false;
        cart.PromoDiscount = 0;
        cart.Promo = null;
        await _context.SaveChangesAsync();

        return Json(new { message = "Success" });
    }
}