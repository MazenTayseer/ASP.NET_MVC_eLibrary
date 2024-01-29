using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MvcLibrary.Models;

namespace MvcLibrary.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (TempData.ContainsKey("ErrorMessage")) ViewBag.ErrorMessage = TempData["ErrorMessage"]!.ToString()!;
        
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}