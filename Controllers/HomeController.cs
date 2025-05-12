using System.Diagnostics;
using MartinsHidraulica.Data;
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
    
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger,
        UserManager<Usuarios> userManager, 
        ApplicationDbContext context)
    {
        _logger = logger;
        _userManager = userManager;
        _context = context;
    }
    
    public IActionResult Index()
    {
        return View();
    }

    //Dash de orçamentos por status
    [HttpGet]
    public IActionResult GetOrcamentosPorStatus(string status)
    {
        var query = _context.Orcamentos.AsQueryable();
        
        if (!string.IsNullOrEmpty(status) && status != "Todos")
        {
            if (status == "Aprovado")
                query = query.Where(o => o.Aprovado == true);
            else if (status == "Reprovado")
                query = query.Where(o => o.Aprovado == false);
            else if (status == "Pendente")
                query = query.Where(o => o.Aprovado == null);
        }
        
        // Contagens por status real (true/false/null)
        var aprovados  = query.Count(o => o.Aprovado == true);
        var reprovados = query.Count(o => o.Aprovado == false);
        var pendentes  = query.Count(o => o.Aprovado == null);

        return Json(new
        {
            aprovados,
            reprovados,
            pendentes
        });
    }
    
    //Dash de vendedores por orçamento aprovado
    [HttpGet]
    public IActionResult GetOrcamentosAprovadosPorVendedor()
    {
        var dados = _context.Orcamentos
            .Where(o => o.Aprovado == true)
            .GroupBy(o => o.Vendedor)
            .Select(g => new
            {
                Vendedor = g.Key,
                TotalAprovados = g.Count()
            })
            .ToList();

        return Json(dados);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}