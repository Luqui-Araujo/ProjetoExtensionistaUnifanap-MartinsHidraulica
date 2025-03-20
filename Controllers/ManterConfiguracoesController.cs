using MartinsHidraulica.Data;
using MartinsHidraulica.Models;
using MartinsHidraulica.Views.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MartinsHidraulica.Controllers;

public class ManterConfiguracoesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ManterConfiguracoesController(
        ApplicationDbContext context)
    {
        _context = context;
    }
    
    //View lista de configurações
    [Authorize(Roles = "Administrador")]
    public IActionResult Configuracoes()
    {
        return View();
    }
    
    //View empresa
    [Authorize(Roles = "Administrador")]
    public IActionResult CadastroEmpresa()
    {
        var empresa = _context.Empresas.FirstOrDefault();
        ViewBag.Empresa = empresa;
        ViewBag.TemEmpresa = empresa != null;
        return View();
    }

    //View Configuracoes orcamento
    [Authorize(Roles = "Administrador")]
    public IActionResult ConfiguracoesOrcamento()
    {
        return View();
    }
    
    //Get para listar as empresas
    public IActionResult ListarEmpresa()
    {
        var empresa = _context.Empresas.ToList();
        return Json(empresa);
    }
    
    //Post Adicionar Empresa
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdicionarEmpresa(EmpresaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Dados inválidos");
        }

        byte[] logoBytes = null;
        if (model.LogoEmpresaFile != null && model.LogoEmpresaFile.Length > 0)
        {
            using var ms = new MemoryStream();
            await model.LogoEmpresaFile.CopyToAsync(ms);
            logoBytes = ms.ToArray();
        }
        
        var Addempresa = new Empresa
        {
            NomeEmpresa = model.NomeEmpresa,
            Cnpj = model.Cnpj,
            RazaoSocial = model.RazaoSocial,
            Cep = model.Cep,
            Endereco = model.Endereco,
            Complemento = model.Complemento,
            Telefone = model.Telefone,
            Email = model.Email,
            LogoEmpresa = logoBytes
        };
            
        _context.Empresas.Add(Addempresa);
        await _context.SaveChangesAsync();
        
        return RedirectToAction("CadastroEmpresa", "ManterConfiguracoes");
    }
    
    //Get Obter informações da empresa cadastrada
    [HttpGet]
    public IActionResult ObterEmpresa(int id)
    {
        var empresa = _context.Empresas.FirstOrDefault(e => e.Id == id);
        
        if (empresa == null)
        {
            return BadRequest(new { success = false, message = "Nenhuma empresa foi encontrada" });
        }
        
        return Json(empresa);
    }
    
    //Post Edição de empresa
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarDadosEmpresa(EmpresaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, message = "Dados inválidos", errors });
        }

        var empresaExistente = await _context.Empresas.FindAsync(model.Id);
        if (empresaExistente == null)
        {
            return BadRequest(new { success = false, message = "Empresa não encontrada" });
        }
        
        empresaExistente.NomeEmpresa = model.NomeEmpresa;
        empresaExistente.RazaoSocial = model.RazaoSocial;
        empresaExistente.Cnpj = model.Cnpj;
        empresaExistente.Cep = model.Cep;
        empresaExistente.Endereco = model.Endereco;
        empresaExistente.Complemento = model.Complemento;
        empresaExistente.Telefone = model.Telefone;
        empresaExistente.Email = model.Email;
        
        //Se houver um novo arquivo enviado atualiza
        if (model.LogoEmpresaFile != null && model.LogoEmpresaFile.Length > 0)
        {
            using var ms = new MemoryStream();
            await model.LogoEmpresaFile.CopyToAsync(ms);
            empresaExistente.LogoEmpresa = ms.ToArray();
        }
        
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Empresa atualizada com sucesso!" });

    }
    
    //Manunteção de departamento
    //Post Adicionar novo departamento
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AdicionarDepartamento(Departamentos departamento)
    {
        if (!ModelState.IsValid)
        {
            var erros = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return BadRequest(new { sucesso = false, mensagem = "Erro ao adicionar o departamento.", erros });
        }

        // Define o departamento como ativo
        departamento.Ativo = true;
        _context.Departamentos.Add(departamento);
        _context.SaveChanges();
        
        return Json(new
        {
            success = true,
            message = "Departamento adicionado com sucesso!",
            id = departamento.IdDepartamento,  // Enviamos o ID do novo departamento
            nome = departamento.NomeDepartamento // Enviamos o nome do novo departamento
        });
    }
    
    //Get Lista departamentos cadastrados na empresa
    [HttpGet]
    public IActionResult ListarDepartamentos(int empresaId)
    {
        var departamentos = _context.Departamentos
            .Where(d => d.EmpresaId == empresaId)
            .Where(d => d.Ativo == true)
            .Select(d => new
            {
                d.IdDepartamento,
                d.NomeDepartamento,
                d.Ativo
            });
        
        return Json(departamentos);
    }
    
    //Post Edição de departamentos
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditarDepartamento([FromBody] Departamentos departamento)
    {
        
        if (departamento == null || string.IsNullOrEmpty(departamento.NomeDepartamento))
        {
            return Json(new { success = false, message = "Dados inválidos." });
        }

        try
        {
            var departamentoExistente = _context.Departamentos.Find(departamento.IdDepartamento);
            if (departamentoExistente != null)
            {
                departamentoExistente.NomeDepartamento = departamento.NomeDepartamento;
                _context.SaveChanges();
                bool atualizado = true;
                
                if (atualizado)
                {
                    return Json(new { success = true, message = "Departamento atualizado com sucesso" });
                }
                else
                {
                    return Json(new { success = false, message = "Falha ao atualizar departamento" });
                }
            }
        }
        
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Erro interno: " + ex.Message });
        }

        if (ModelState.IsValid)
        {
            try
            {
                var departamentoExistente = _context.Departamentos.Find(departamento.IdDepartamento);
                if (departamentoExistente != null)
                {
                    departamentoExistente.NomeDepartamento = departamento.NomeDepartamento;
                    _context.SaveChanges();
                    return Json(new { sucess = true, mensagem = "Departamento editado com sucesso!" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { sucess = false, mensagem = "Erro ao editar o departamento." });
            }
        }
                
        return BadRequest(new {sucess = false, mensagem = "Erro ao editar o departamento." });
    }
    
    //Post Inativar departaemnto
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult InativarDepartamento([FromBody] Departamentos departamento)
    {
        var departamentoExistente = _context.Departamentos.Find(departamento.IdDepartamento);
        if (departamentoExistente != null)
        {
            departamentoExistente.Ativo = false;
            _context.SaveChanges();
            return Json(new { success = true, mensagem = "Departamento inativado com sucesso!" });
        }

        return BadRequest(new { success = false, mensagem = "Erro ao inativar o departamento." });
    }
    
    //Métodos
    //Obter a logo em qualquer parte do sistema
    public IActionResult ObterLogo()
    {
        var empresa = _context.Empresas.FirstOrDefault();
        if (empresa == null || empresa.LogoEmpresa == null)
        {
            return NotFound();
        }
        
        return File(empresa.LogoEmpresa, "image/png");
    }
    
    //Configurações de orçamento
    
    //POST adicionar novo vendedor
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AdicionarVendedor(ConfiguracoesViewModel model)
    {
        try
        {
            var vendedor = new Vendedores
            {
                Id = model.IdVendedor,
                Nome = model.NomeVendedor,
                Ativo = model.AtivoVendedor = true
            };

            _context.Vendedores.Add(vendedor);
            _context.SaveChanges();

            return Json(new { success = true, message = "Vendedor adicionado com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Erro ao adicionar o vendedor.", erros = new[] { ex.Message } });

        }
    }
    
    //POST adicionar novo tipo de pagamento
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AdicionarTipoPagamento(ConfiguracoesViewModel model)
    {
        try
        {
            var tipoPagamento = new TiposPagamento
            {
                Id = model.IdTipoPagamento,
                Nome = model.NomeTipoPagamento,
                Ativo = model.AtivoTipoPagamento = true
            };
            
            _context.TiposPagamento.Add(tipoPagamento);
            _context.SaveChanges();
            
            return Json(new {success = true, message = "Tipo de pagamento adicionado com sucesso!"});
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Erro ao adicionar o vendedor.", erros = new[] { ex.Message } });

        }
    }
    
    //GET listar vendedores e tipos de pagamentos
    [HttpGet]
    public IActionResult ListarTiposDePagamentoEVendedores()
    {
        var vendedores = _context.Vendedores
            .Where(v => v.Ativo == true)
            .Select(v => new
            {
                v.Id,
                v.Nome,
                v.Ativo
            });
        
        var tiposPagamento = _context.TiposPagamento
            .Where(t => t.Ativo == true)
            .Select(t => new
            {
                t.Id,
                t.Nome,
                t.Ativo
            });

        return Json(new
        {
            vendedores = vendedores,
            tiposPagamento = tiposPagamento
        });
    }
    
    //POST Editar vendedor
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditarVendedor(int id, string nome)
    {
        try
        {
            var vendedor = _context.Vendedores.Find(id);
            if (vendedor == null)
            {
                return Json(new { success = false, message = "Vendedor não encontrado" });
            }

            vendedor.Nome = nome;
            _context.SaveChanges();

            return Json(new { success = true, message = "Vendedor atualizado com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Erro ao atualizar o vendedor.", erros = new[] { ex.Message } });
        }
    }

    //POST Editar tipo de pagamento
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditarTipoPagamento(int id, string nome)
    {
        try
        {
            var tipoPagamento = _context.TiposPagamento.Find(id);
            if (tipoPagamento == null)
            {
                return Json(new { success = false, message = "Tipo de pagamento não encontrado" });
            }

            tipoPagamento.Nome = nome;
            _context.SaveChanges();

            return Json(new { success = true, message = "Tipo de pagamento atualizado com sucesso!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Erro ao atualizar o tipo de pagamento.", erros = new[] { ex.Message } });
        }
    }
    
    
}