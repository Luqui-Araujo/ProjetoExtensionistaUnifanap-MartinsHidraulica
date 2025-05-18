using System.ComponentModel.DataAnnotations;

namespace MartinsHidraulica.Views.ViewModel;

public class MudarSenhaAdminViewModel
{
    [Required]
    public string IdUsuario { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Nova Senha")]
    public string NovaSenha { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [Compare("NovaSenha", ErrorMessage = "A nova senha e a confirmação não são iguais.")]
    public string ConfirmarSenha { get; set; }
}