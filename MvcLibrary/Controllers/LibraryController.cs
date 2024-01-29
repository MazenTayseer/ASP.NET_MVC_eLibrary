using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcLibrary.Data;
using MvcLibrary.Models;

namespace MvcLibrary.Controllers;

public class LibraryController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public LibraryController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var books = _context.Book.ToList();

        var random = new Random();
        var recommendedBooks = books.OrderBy(b => random.Next()).Take(4).ToList();

        var pickForToday = books.First();

        ViewBag.RecommendedBooks = recommendedBooks;
        ViewBag.Books = books;
        ViewBag.PickForToday = pickForToday;

        if (TempData.ContainsKey("ErrorMessage")) ViewBag.ErrorMessage = TempData["ErrorMessage"]!.ToString()!;

        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddToCart(int bookId)
    {
        var book = await _context.Book.FindAsync(bookId);

        var user = await _userManager.GetUserAsync(User);
        
     
        
        //Retrieves the user's shopping cart
        //Contains nav properties for Cart items and Book

        var cart = await _context.Cart
            .Include(c => c.CartItems!)
            .ThenInclude(ci => ci.Book)
            .FirstOrDefaultAsync(c => c.User!.Id == user!.Id);

        if (cart != null)
            
            
            
        {
            var existingCartItem = cart.CartItems!.FirstOrDefault(ci => ci.Book != null && ci.Book.Id == bookId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity++;
            }
            else
            {
                var newCartItem = new CartItem
                {
                    Cart = cart,
                    Book = book,
                    Quantity = 1
                };

                cart.CartItems!.Add(newCartItem);
            }


            cart.CartTotalPrice = cart.CartItems!.Sum(item => item.Book!.Price * item.Quantity);
            cart.CartTotalItems = cart.CartItems!.Sum(item => item.Quantity);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Book added to cart successfully" });
        }

        return Json(new { success = false, message = "Cart not found for the user" });
    }


    [Authorize]
    public async Task<IActionResult> AddReview(int id)
    {
        var book = await _context.Book.FindAsync(id);
        var user = await _userManager.GetUserAsync(User);

        ViewData["BookName"] = book!.Name;

        var existingReview = await _context.Review
            .FirstOrDefaultAsync(r => r.Book!.Id == id && r.User!.Id == user!.Id);

        if (existingReview != null)
        {
            TempData["ErrorMessage"] = "You have already added a rating to this book.";
            return RedirectToAction("Index");
        }

        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddReview(int id, Review review)
    {
        var book = await _context.Book.FindAsync(id);

        var user = await _userManager.GetUserAsync(User);


        ModelState.Remove("User");
        ModelState.Remove("Book");
        if (ModelState.IsValid)
        {
            var newReview = new Review
            {
                User = user!,
                Book = book!,
                Rating = review.Rating,
                Comment = review.Comment
            };

            await _context.Review.AddAsync(newReview);
            await _context.SaveChangesAsync();

            var bookReviews = await _context.Book.Include(b => b.BookReviews).FirstOrDefaultAsync(bi => id == bi.Id);
            if (bookReviews!.BookReviews!.Any())
            {
                var totalRatings = bookReviews.BookReviews!.Sum(b => b.Rating);
                var avgRating = (decimal)totalRatings / bookReviews.BookReviews!.Count;

                book!.CumulativeRating = avgRating;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        return View(review);
    }
}