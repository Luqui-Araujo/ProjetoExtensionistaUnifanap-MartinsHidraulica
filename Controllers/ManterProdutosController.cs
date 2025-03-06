using DocumentFormat.OpenXml.InkML;
using MartinsHidraulica.Data;
using MartinsHidraulica.Models;
using MartinsHidraulica.Views.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MartinsHidraulica.Controllers;

public class ManterProdutosController : Controller
{
    private readonly ApplicationDbContext _context;
    public ManterProdutosController
        (ApplicationDbContext context)
    {
        _context = context;
    }
    
    // GET
    public IActionResult Produtos()
    {
        return View();
    }
    
    //Post Cadastro de Produtos
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarProduto(ProdutoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { success = false, message = errors });
        }

        var addProduto = new Produto
        {
            Nome = model.Nome,
            Preco = model.Preco,
            Descricao = model.Descricao,
            Ativo = model.Ativo = true
        };
        
        _context.Produtos.Add(addProduto);
        await _context.SaveChangesAsync();
        
        return Json(new { success = true, message = "Produto cadastrado com sucesso!" });
    }
    
    //GET obter informações por produto
    [HttpGet]
    [ValidateAntiForgeryToken]
    public IActionResult ObterProdutos(int id)
    {
        var produto = _context.Produtos.Find(id);

        if (produto == null)
        {
            return NotFound("Produto não encontrado");
        }

        return Json(produto);
    }
    
    //POST listar produtos ativos
    [HttpGet]
    public IActionResult ListarProdutosAtivos()
    {
        var produtos = _context.Produtos
            .Where(p => p.Ativo == true)
            .ToList();
        return Json(produtos);
    }
    
    //POST listar produtos inativos
    [HttpGet]
    public IActionResult ListarProdutosInativos()
    {
        var produtos = _context.Produtos
            .Where(p => p.Ativo == false)
            .ToList();
        return Json(produtos);
    }
    
    //POST Inativar produto
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Inativar(int id)
    {
        
        var produto = _context.Produtos.Find(id);
        if (produto != null)
        {
            produto.Ativo = false;
            _context.SaveChanges();
            return Json(new {success = true, message = "Inativado com sucesso!"} );
        }

        return BadRequest("Usuário não encontrado");
    }

    //POST Reativar produto
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reativar(int id)
    {
        
        var produto = _context.Produtos.Find(id);
        if (produto != null)
        {
            produto.Ativo = true;
            _context.SaveChanges();
            return Json(new {success = true, message = "Reativado com sucesso!"} );
        }

        return BadRequest("Usuário não encontrado");
    }
    
    //POST editar produto
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarProduto(ProdutoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, message = "Dados inválidos", errors });
        }
        
        var produtoExistente = await _context.Produtos.FindAsync(model.Id);
        if (produtoExistente == null)
        {
            return BadRequest("Produto não encontrado");
        }
        
        produtoExistente.Nome = model.Nome;
        produtoExistente.Preco = model.Preco;
        produtoExistente.Descricao = model.Descricao;
        
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Produto atualizado com sucesso!" });
    }
}