using MartinsHidraulica.Data;
using MartinsHidraulica.Views.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MartinsHidraulica.Controllers;

public class ManterOrcamentosController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public ManterOrcamentosController
        (ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Orcamentos()
    {
        return View();
    }

    public IActionResult NovoOrcamento()
    {
        var empresa = _context.Empresas
            .FirstOrDefault();
        var tipoPagamento = _context.TiposPagamento
            .Where(t => t.Ativo == true)
            .ToList();

        var viewModel = new OrcamentoViewModel
        {
            EmpresaNome = empresa?.NomeEmpresa,
            EmpresaRazaoSocial = empresa?.RazaoSocial,
            EmpresaCnpj = empresa?.Cnpj,
            EmpresaCep = empresa?.Cep,
            EmpresaTelefone = empresa?.Telefone,
            EmpresaEmail = empresa?.Email,
            EmpresaEndereco = empresa?.Endereco,
            EmpresaComplemento = empresa?.Complemento,
            CondicaoPagamentos = tipoPagamento,
        };
        
        return View(viewModel);
    }
    
    
}