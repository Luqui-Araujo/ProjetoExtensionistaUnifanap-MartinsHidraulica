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

        return Json(new { success = true, message = "Cliente adicionado com sucesso!" });
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
    
    //Post Inativar cliente
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Inativar(int id)
    {
        
        var cliente = _context.Clientes.Find(id);
        if (cliente != null)
        {
            cliente.Ativo = false;
            _context.SaveChanges();
            return Json(new {success = true, message = "Inativado com sucesso!"} );
        }

        return BadRequest("Usuário não encontrado");
    }

    //Post Reativar cliente
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reativar(int id)
    {
        
        var cliente = _context.Clientes.Find(id);
        if (cliente != null)
        {
            cliente.Ativo = true;
            _context.SaveChanges();
            return Json(new {success = true, message = "Reativado com sucesso!"} );
        }

        return BadRequest("Usuário não encontrado");
    }
    
    //Post edição de Clientes
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCliente(ClienteViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, message = "Dados inválidos", errors });
        }
        
        var clienteExistente = await _context.Clientes.FindAsync(model.Id);
        if (clienteExistente == null)
        {
            return BadRequest("Cliente não encontrado");
        }
        
        clienteExistente.Nome = model.Nome;
        clienteExistente.CpfCnpj = model.CpfCnpj;
        clienteExistente.InscricaoEstadual = model.InscricaoEstadual;
        clienteExistente.Email = model.Email;
        clienteExistente.Telefone = model.Telefone;
        clienteExistente.Cep = model.Cep;
        clienteExistente.Endereco = model.Endereco;
        clienteExistente.Bairro = model.Bairro;
        clienteExistente.Cidade = model.Cidade;
        clienteExistente.Estado = model.Estado;
        clienteExistente.Ativo = model.Ativo = true;
        clienteExistente.TipoCliente = model.TipoCliente;
        
        await _context.SaveChangesAsync();
        
        return Json(new { success = true, message = "Cliente editado com sucesso!"});
        
    }
    
    //Listare orçamentos do cliente
    [HttpGet]
    public IActionResult ListarOrcamentosDoCliente(int clienteId, int pagina = 1, DateTime? dataInicial = null, DateTime? dataFinal = null)
    {
        int tamanhoPagina = 10;

        var query = _context.Orcamentos
            .Where(o => o.ClienteId == clienteId);

        if (dataInicial.HasValue)
        {
            // Pega o início do dia
            DateTimeOffset dtIni = dataInicial.Value.Date.ToUniversalTime();
            query = query.Where(o => o.DataOrcamento >= dtIni);
        }

        if (dataFinal.HasValue)
        {
            // Pega o final do dia
            DateTimeOffset dtFim = dataFinal.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime();
            query = query.Where(o => o.DataOrcamento <= dtFim);
        }
        
        var total = query.Count();
        
        var orcamentos = query
            .OrderByDescending(o => o.DataOrcamento)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToList();

        return Json(new
        {
            orcamentos,
            total,
            paginaAtual = pagina,
            totalPaginas = (int)Math.Ceiling((double)total / tamanhoPagina)
        });
    }
    
}