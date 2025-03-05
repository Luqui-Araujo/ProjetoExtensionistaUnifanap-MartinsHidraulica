using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MartinsHidraulica.Models;

public class Usuarios : IdentityUser
{
    [Required(ErrorMessage = "O departamento é obrigatório!")]
    public string Departamento { get; set; }
    [Required(ErrorMessage = "O tipo do usuário e obrigatório!")]
    public string TipoUsuario { get; set; }
    [Required(ErrorMessage = "Nome do usuário é obrigatório")]
    public string Nome { get; set; }
    public bool Ativo { get; set; }
}