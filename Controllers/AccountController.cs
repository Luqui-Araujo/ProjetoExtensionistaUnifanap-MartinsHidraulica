using Microsoft.AspNetCore.Mvc;

namespace MartinsHidraulica.Controllers;

public class AccountController : Controller
{
    // GET
    public IActionResult AcessoNegado()
    {
        return View();
    }
}