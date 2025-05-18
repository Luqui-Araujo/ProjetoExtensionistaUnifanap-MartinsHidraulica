using System.ComponentModel.DataAnnotations;

namespace MartinsHidraulica.Views.ViewModel;

public class MudarSenhaViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Senha Atual")]
    public string SenhaAtual { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Nova Senha")]
    public string NovaSenha { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [Compare("NovaSenha", ErrorMessage = "A nova senha e a confirmação não são iguais.")]
    [Display(Name = "Confirmar Senha")]
    public string ConfirmarSenha { get; set; }
}