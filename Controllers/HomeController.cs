using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MartinsHidraulica.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MartinsHidraulica.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly UserManager<Usuarios> _userManager;
    
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, UserManager<Usuarios> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }
    
    public IActionResult Index()
    {
        return View();
    }
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}