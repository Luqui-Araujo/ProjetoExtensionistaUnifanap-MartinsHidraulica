using System.ComponentModel.DataAnnotations;

namespace MartinsHidraulica.Views.ViewModel;

public class ConfiguracoesViewModel
{
    public int IdVendedor { get; set; }
    public int IdTipoPagamento { get; set; }
    
    [Required(ErrorMessage = "Preencha o campo Nome do Vendedor")]
    public string NomeVendedor {get; set;}
    [Required(ErrorMessage = "Preencha o campo Nome do Tipo de Pagamento")]
    public string NomeTipoPagamento {get; set;}
    public bool AtivoVendedor { get; set; }
    public bool AtivoTipoPagamento { get; set; }
}