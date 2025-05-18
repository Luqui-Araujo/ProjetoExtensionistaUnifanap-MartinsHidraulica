using MartinsHidraulica.Data;
using MartinsHidraulica.Models;
using MartinsHidraulica.Views.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MartinsHidraulica.Controllers;

[Authorize]
public class ManterUsuariosController : Controller
{
    private readonly SignInManager<Usuarios> _signInManager;
    private readonly UserManager<Usuarios> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    private readonly ApplicationDbContext _context;

    public ManterUsuariosController(
        SignInManager<Usuarios> signInManager,
        UserManager<Usuarios> userManager,
        ApplicationDbContext context,
        RoleManager<IdentityRole> roleManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    //Get Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpGet]
    public IActionResult MudarSenha()
    {
        return View(new MudarSenhaViewModel());
    }

    //Post Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Autenticar(LoginVIewModel model)
    {
        var usuario = await _userManager.FindByEmailAsync(model.Email);

        if (usuario == null || !usuario.Ativo)
        {
            TempData["ErrorMessage"] = "Usuário Inativo ou não Encontrado";
            return RedirectToAction("Login");
        }
        
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Senha,
                isPersistent: false,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "E-mail ou senha incorretos!";
        }

        return RedirectToAction("Login");
    }

    //Get Usuarios Registro
    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public IActionResult Usuarios()
    {
        return View();
    }

    //Post Registrar usuários
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(Usuarios usuario, string senha)
    {
        if (ModelState.IsValid)
        {
            var usuarioExistente = await _userManager.FindByEmailAsync(usuario.Email);

            if (usuarioExistente != null)
            {
                return Json(new { success = false, message = new[] { "E-mail já cadastrado" } });
            }

            var user = new Usuarios
            {
                UserName = usuario.Email,
                Email = usuario.Email,
                Nome = usuario.Nome,
                Departamento = usuario.Departamento,
                TipoUsuario = usuario.TipoUsuario,
                Ativo = true
            };

            var result = await _userManager.CreateAsync(user, senha);
            
            if (!result.Succeeded)
            {
                // Se falhar, retorna os erros
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                var errorsModel = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, errors = errorsModel });
            }
            
            //Definir a role do usuário
            var role = usuario.TipoUsuario;

            if (!await _roleManager.RoleExistsAsync(role))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
                if (!roleResult.Succeeded)
                {
                    return Json(new { success = false, message = new[] { "Não foi possível criar a role" } });
                }
            }
            
            await _userManager.AddToRoleAsync(user, role);

            if (result.Succeeded)
            {
                return Json(new { success = true, redirectUrl = Url.Action("Usuarios") });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return RedirectToAction("Usuarios");
        }

        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        return Json(new { success = false, errors = errors });
    }

    //Listar usuários ativos
    public IActionResult ListarAtivos()
    {
        var usuariosAtivos = _context.Users.Where(u => u.Ativo).ToList();
        return Json(usuariosAtivos);
    }

    //Listar usuários inativos
    public IActionResult ListarInativos()
    {
        var usuariosInativos = _context.Users.Where(u => !u.Ativo).ToList();
        return Json(usuariosInativos);
    }

    //Post Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    //Post Inativar usuário
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Inativar(string id)
    {
        var usuario = _context.Users.Find(id);
        if (usuario != null)
        {
            usuario.Ativo = false;
            _context.SaveChanges();
            return Ok(); // Retorna 200 OK
        }

        return BadRequest("Usuário não encontrado");
    }

    //Post Reativar Usuário
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reativar(string id)
    {
        var usuario = _context.Users.Find(id);
        if (usuario != null)
        {
            usuario.Ativo = true;
            _context.SaveChanges();
            return Ok();
        }

        return BadRequest("Usuário não encontrado");
    }

    //Post edição de usuários
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Usuarios usuario)
    {
        if (ModelState.IsValid)
        {
            var usuarioExistente = await _userManager.FindByIdAsync(usuario.Id);
            if (usuarioExistente != null)
            {
                usuarioExistente.Nome = usuario.Nome;
                usuarioExistente.Email = usuario.Email;
                usuarioExistente.UserName = usuario.Email;
                usuarioExistente.Departamento = usuario.Departamento;
                usuarioExistente.TipoUsuario = usuario.TipoUsuario;

                var result = await _userManager.UpdateAsync(usuarioExistente);
                if (result.Succeeded)
                {
                    return RedirectToAction("Usuarios");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
        }

        return View("Usuarios");
    }
    
    //Post o mudar senha usuário
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MudarSenha(MudarSenhaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var usuario = await _userManager.GetUserAsync(User);
        
        if (usuario == null)
        {
            //Se bugar e não encontrar o usuário, redireciona para o login
            return RedirectToAction("Login", "ManterUsuarios");
        }
        
        var result = await _userManager.ChangePasswordAsync(usuario, model.SenhaAtual, model.NovaSenha);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }
        
        //Desloga após mudar a senha
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "ManterUsuarios");
    }
    
    //Post o mudar senha admin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MudarSenhaAdmin(MudarSenhaAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index", "Home");
        }
        
        var usuario = await _userManager.FindByIdAsync(model.IdUsuario);
        if (usuario == null)
        {
            TempData["Error"] = "Usuário não encontrado";
            return RedirectToAction("Usuarios");
        }
        
        //Tem que gerar um token antes de resetar
        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var result = await _userManager.ResetPasswordAsync(usuario, token, model.NovaSenha);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" / ", result.Errors.Select(e => e.Description));
        }
        else
        {
            @TempData["Success"] = "Senha alterada com sucesso!";
        }
        
        return RedirectToAction("Usuarios");
    }
    
    public IActionResult ObterDepartamentos(Departamentos departamentos)
    {
        var departamentosEmpresa = _context.Departamentos
            .Where(d => d.Ativo == true)
            .Select(d => new
            {
                nomeDepartamento = d.NomeDepartamento
            })
            .ToList();

        if (departamentosEmpresa == null || !departamentosEmpresa.Any())
        {
            return BadRequest(new { success = false, message = "Nenhum departamento encontrado" });
        }
        
        return Json(departamentosEmpresa);
    }
}