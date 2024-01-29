using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcLibrary.Data;
using MvcLibrary.Models;
using Newtonsoft.Json;

namespace MvcLibrary.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CheckoutController(ApplicationDbContext context, UserManager<IdentityUser> userManager,
        IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        var cart = await _context.Cart
            .Include(c => c.CartItems!)
            .ThenInclude(ci => ci.Book)
            .FirstOrDefaultAsync(c => c.User!.Id == user!.Id);

        var cartItems = cart!.CartItems!.ToList();

        ViewBag.CartItems = cartItems;

        ViewData["Cart Total Items"] = cart.CartItems!.Sum(item => item.Quantity);

        if (cart.PromoApplied)
        {
            ViewBag.PromoIsApplied = true;
            ViewData["Cart Total Price"] = Math.Round(cart.CartTotalPrice * (1 - cart.PromoDiscount), 2);
        }
        else
        {
            ViewData["Cart Total Price"] = Math.Round(cart.CartTotalPrice, 2);
        }

        if (cart.CartTotalItems == 0)
        {
            TempData["ErrorMessage"] = "Cart must have items to proceed to checkout.";
            return RedirectToAction("Index", "Cart");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        var user = await _userManager.GetUserAsync(User);

        var cart = await _context.Cart.Include(p => p.Promo)
            .Include(c => c.CartItems!)
            .ThenInclude(ci => ci.Book)
            .FirstOrDefaultAsync(c => c.User!.Id == user!.Id);

        var cartItems = cart!.CartItems!.ToList();

        ViewBag.CartItems = cartItems;

        ViewData["Cart Total Price"] = cart.CartItems!.Sum(item => item.Book!.Price * item.Quantity);
        ViewData["Cart Total Items"] = cart.CartItems!.Sum(item => item.Quantity);

        ModelState.Remove("User");
        ModelState.Remove("OrderTotalPrice");
        ModelState.Remove("OrderTotalItems");
        ModelState.Remove("OrderItems");
        ModelState.Remove("PayPalDetails");

        if (ModelState.IsValid)
        {
            var newOrder = new Order
            {
                User = user!,
                FirstName = order.FirstName,
                LastName = order.LastName,
                Address = order.Address,
                Country = order.Country,
                City = order.City,
                ZipCode = order.ZipCode,
                OrderTotalPrice = cart.CartItems!.Sum(item => item.Book!.Price * item.Quantity),
                OrderTotalItems = cart.CartItems!.Sum(item => item.Quantity),
                PayPalDetails = order.PayPalDetails
            };

            _context.Order.Add(newOrder);

            foreach (var item in cartItems)
            {
                var newOrderItem = new OrderItem
                {
                    Book = item.Book,
                    Order = newOrder,
                    Quantity = item.Quantity
                };

                var currentBook = await _context.Book.FindAsync(item.Book!.Id);
                currentBook!.Stock--;

                if (cart.PromoApplied)
                {
                    var appliedPromoCode =
                        await _context.Promo.FirstOrDefaultAsync(p => p.PromoId == cart.Promo!.PromoId);
                    appliedPromoCode!.Used = true;

                    cart.PromoApplied = false;
                    cart.PromoDiscount = 0;
                    cart.Promo = null;
                }

                _context.OrderItem.Add(newOrderItem);
            }

            _context.CartItem.RemoveRange(cartItems);
            cart.CartTotalItems = 0;
            cart.CartTotalPrice = 0;
            await _context.SaveChangesAsync();

            SendOrderEmail(user!.Email!, "Thank You For Your Purchase", order);
            return RedirectToAction("Index", "Home");
        }

        return View("Index", order);
    }

    private void SendOrderEmail(string toEmail, string subject, Order order)
    {
        try
        {
            var htmlFileName = "EmailOrderTemplateOld.html";
            var emailTemplatesPath = Path.Combine(_webHostEnvironment.WebRootPath, "EmailTemplates");
            var filePath = Path.Combine(emailTemplatesPath, htmlFileName);

            var htmlBody = System.IO.File.ReadAllText(filePath);

            var paypalDetailsObject = JsonConvert.DeserializeObject<dynamic>(order.PayPalDetails!);


            string paidAmount = paypalDetailsObject!["purchase_units"][0]["amount"]["currency_code"] + " " +
                                paypalDetailsObject["purchase_units"][0]["amount"]["value"];
            string paypalId = paypalDetailsObject["id"];
            string paypalTime = paypalDetailsObject["create_time"] + " GMT";

            htmlBody = htmlBody.Replace("{{ Total Paid }}", paidAmount);
            htmlBody = htmlBody.Replace("{{ Transaction_ID }}", paypalId);
            htmlBody = htmlBody.Replace("{{ Transaction_Date }}", paypalTime);

            var message = new MailMessage("ebiblioteca.elibrary@gmail.com", toEmail);
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = htmlBody;

            var smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;

            var networkCredential = new NetworkCredential("ebiblioteca.elibrary@gmail.com", "");
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = networkCredential;
            smtp.Send(message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public IActionResult OrderCompleted()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CheckDetails([FromBody] Order orderData)
    {
        if (ModelState.IsValid) return Json(new { message = "Success" });

        return Json(new
            { message = "Fail", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
    }
}