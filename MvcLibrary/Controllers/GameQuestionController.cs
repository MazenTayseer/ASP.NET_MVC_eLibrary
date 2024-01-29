using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcLibrary.Data;
using MvcLibrary.Models;

namespace MvcLibrary.Controllers;

[Authorize(Roles = "Admin")]
public class GameQuestionController : Controller
{
    private readonly ApplicationDbContext _context;

    public GameQuestionController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: GameQustion
    public IActionResult Index()
    {
        var questions = _context.GameQuestion.ToList();
        
        ViewBag.Questions = questions;
        return View();
    }

    // GET: GameQustion/Details/5
    
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var gameQuestion = await _context.GameQuestion
            .FirstOrDefaultAsync(m => m.GameQuestionId == id);
        if (gameQuestion == null) return NotFound();

        return View(gameQuestion);
    }

    // GET: GameQustion/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: GameQustion/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("GameQuestionId,Question,Answer")] GameQuestion gameQuestion)
    {
        if (ModelState.IsValid)
        {
            _context.Add(gameQuestion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(gameQuestion);
    }

    // GET: GameQustion/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var gameQuestion = await _context.GameQuestion.FindAsync(id);
        if (gameQuestion == null) return NotFound();
        return View(gameQuestion);
    }

    // POST: GameQustion/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("GameQuestionId,Question,Answer")] GameQuestion gameQuestion)
    {
        if (id != gameQuestion.GameQuestionId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(gameQuestion);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GameQuestionExists(gameQuestion.GameQuestionId))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(gameQuestion);
    }

    // GET: GameQustion/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var gameQuestion = await _context.GameQuestion
            .FirstOrDefaultAsync(m => m.GameQuestionId == id);
        if (gameQuestion == null) return NotFound();

        return View(gameQuestion);
    }

    // POST: GameQustion/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var gameQuestion = await _context.GameQuestion.FindAsync(id);
        if (gameQuestion != null) _context.GameQuestion.Remove(gameQuestion);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool GameQuestionExists(int id)
    {
        return _context.GameQuestion.Any(e => e.GameQuestionId == id);
    }
}