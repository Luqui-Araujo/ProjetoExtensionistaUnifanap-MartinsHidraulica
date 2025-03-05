using System.ComponentModel.DataAnnotations;

namespace MartinsHidraulica.Views.ViewModel;

public class LoginVIewModel
{
    
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Senha é obrigatóra")]
    public string Senha { get; set; }
    
}