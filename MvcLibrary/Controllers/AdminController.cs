using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcLibrary.Data;
using MvcLibrary.Models;

namespace MvcLibrary.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Admin
    public IActionResult Index()
    {
        var books = _context.Book.ToList();
        var users = _context.Users.ToList();

        ViewBag.Books = books;
        ViewBag.Users = users;


        ViewData["TotalBooks"] = books.Count();
        ViewData["TotalUsers"] = users.Count();

        return View();
    }

    // GET: Admin/ViewBook/5
    public async Task<IActionResult> ViewBook(int? id)
    {
        if (id == null) return NotFound();

        var book = await _context.Book
            .FirstOrDefaultAsync(book => book.Id == id);
        
        if (book == null) return NotFound();

        return View(book);
    }

    // GET: Admin/CreateBook
    public IActionResult CreateBook()
    {
        return View();
    }

    // POST: Admin/CreateBook
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBook([Bind("Id,Name,Author,Description,Price,Stock,Image")] Book book)
    {
        foreach (var modelState in ViewData.ModelState.Values)
        foreach (var error in modelState.Errors)
            Console.WriteLine("Error: " + error.ErrorMessage);

        if (ModelState.IsValid)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "imgs/books");

            var uniqueFileName = $"{Guid.NewGuid()}_{book.Name}{Path.GetExtension(book.Image!.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                book.Image?.CopyTo(fileStream);
            }

            book.ImageUrl = "/imgs/books/" + uniqueFileName;
            
            _context.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        return View(book);
    }
    

    // GET: Admin/EditBook/5
    public async Task<IActionResult> EditBook(int? id)
    {
        if (id == null) return NotFound();

        var book = await _context.Book.FindAsync(id);
        if (book == null) return NotFound();
        return View(book);
    }

    // POST: Admin/EditBook/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBook(int id, [Bind("Id,Name,Author,Description,Price,Stock,Image")] Book book)
    {
        if (id != book.Id) return NotFound();

        ModelState.Remove("Image");
        if (ModelState.IsValid)
        {
            try
            {
                var existingBook = await _context.Book.FindAsync(id);

                if (existingBook == null) return NotFound();

                // Update book details except for the image
                existingBook.Name = book.Name;
                existingBook.Author = book.Author;
                existingBook.Description = book.Description;
                existingBook.Price = book.Price;
                existingBook.Stock = book.Stock;

                // Check if a new image has been uploaded
                if (book.Image != null)
                {
                    if (!string.IsNullOrEmpty(existingBook.ImageUrl))
                    {
                        var existingImagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                            existingBook.ImageUrl.TrimStart('/'));

                        Console.WriteLine("Existing : " + existingImagePath);
                        if (System.IO.File.Exists(existingImagePath))
                        {
                            Console.WriteLine("DELETED");
                            System.IO.File.Delete(existingImagePath);
                        }
                    }


                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "imgs/books");

                    var uniqueFileName = $"{Guid.NewGuid()}_{book.Name}{Path.GetExtension(book.Image.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await book.Image.CopyToAsync(fileStream);
                    }

                    existingBook.ImageUrl = "/imgs/books/" + uniqueFileName;
                }

                _context.Update(existingBook);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(book.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction("Index");
        }

        return View(book);
    }

    // GET: Admin/DeleteBook/5
    public async Task<IActionResult> DeleteBook(int? id)
    {
        if (id == null) return NotFound();

        var book = await _context.Book
            .FirstOrDefaultAsync(m => m.Id == id);
        if (book == null) return NotFound();

        return View(book);
    }

    // POST: Admin/DeleteBook/5
    [HttpPost]
    [ActionName("DeleteBook")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Book.FindAsync(id);
        if (book != null)
        {
            _context.Book.Remove(book);

            if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                var existingImagePath = Path.Combine(_webHostEnvironment.WebRootPath, book.ImageUrl.TrimStart('/'));

                if (System.IO.File.Exists(existingImagePath)) System.IO.File.Delete(existingImagePath);
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    private bool BookExists(int id)
    {
        return _context.Book.Any(e => e.Id == id);
    }
}