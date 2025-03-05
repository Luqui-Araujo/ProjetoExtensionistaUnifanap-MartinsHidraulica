using MartinsHidraulica.Data;
using MartinsHidraulica.Models;
using MartinsHidraulica.Views.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MartinsHidraulica.Controllers;

public class ManterClientesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ManterClientesController
        (ApplicationDbContext context)
    {
        _context = context;
    }
    
    //Tela de clientes cadastrados
    public IActionResult Clientes()
    {
        return View();
    }
    
    //Get para listar clientes ativos
    [HttpGet]
    public IActionResult ListarClientesAtivos()
    {
        var clientesAtivos = _context.Clientes
            .Where(c => c.Ativo)
            .ToList();
        return Json(clientesAtivos);
    }
    
    //Get para listar clientes inativos
    [HttpGet]
    public IActionResult ListarClientesInativos()
    {
        var clientesInativos = _context.Clientes
            .Where(c => !c.Ativo)
            .ToList();
        return Json(clientesInativos);
    }
    
    //Cadastro de clientes
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarCliente(ClienteViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { success = false, message = errors });
        }

        var addCliente = new Cliente
        {
            Nome = model.Nome,
            Telefone = model.Telefone,
            Email = model.Email,
            CpfCnpj = model.CpfCnpj,
            InscricaoEstadual = model.InscricaoEstadual,
            Cep = model.Cep,
            Endereco = model.Endereco,
            Bairro = model.Bairro,
            Cidade = model.Cidade,
            Estado = model.Estado,
            Ativo = model.Ativo = true,
            TipoCliente = model.TipoCliente,
        };
        
        _context.Clientes.Add(addCliente);
        await _context.SaveChangesAsync();

        return RedirectToAction("Clientes");
    }
    
    //Get obter informações por cliente
    [HttpGet]
    [ValidateAntiForgeryToken]
    public IActionResult ObterCliente(int id)
    {
        var cliente = _context.Clientes.Find(id);

        if (cliente == null)
        {
            return NotFound(new { success = false, message = "Cliente não encontrado" });
        }

        return Json(cliente);
    }
    
    
}