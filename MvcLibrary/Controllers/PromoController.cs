using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcLibrary.Data;
using MvcLibrary.Models;

namespace MvcLibrary.Controllers;

[Authorize]
public class PromoController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public PromoController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var userExists = await _context.Promo.FirstOrDefaultAsync(p => p.User == user);

        if (userExists != null)
        {
            TempData["ErrorMessage"] = "You have already gained a Promo code.";
            return RedirectToAction("Index", "Home");
        }

        // Get a random question from the database
        var random = new Random();
        var count = _context.GameQuestion.Count();
        var randomQuestion = random.Next(0, count);

        var question = _context.GameQuestion.Skip(randomQuestion).FirstOrDefault();

        if (question != null)
        {
            ViewBag.Question = question.Question!;
            ViewData["QuestionId"] = question.GameQuestionId;
            ViewData["Answer"] = question.Answer;
        }
        else
        {
            ViewBag.Question = "No questions available.";
        }

        return View();
    }

    public async Task<IActionResult> Answer(int questionId, string answer)
    {
        var question = await _context.GameQuestion.FindAsync(questionId);
        var user = await _userManager.GetUserAsync(User);

        if (question != null)
        {
            var correctAnswer = question.Answer;

            if (string.Equals(answer, correctAnswer, StringComparison.OrdinalIgnoreCase))
            {
                var promoCode = Guid.NewGuid().ToString();

                var newPromo = new Promo
                {
                    User = user,
                    Code = promoCode
                };

                _context.Promo.Add(newPromo);
                await _context.SaveChangesAsync();

                return Json(new { message = "Correct", promo = promoCode });
            }

            return Json(new { message = "Incorrect" });
        }

        return Json(new { message = "Question not found." });
    }
}