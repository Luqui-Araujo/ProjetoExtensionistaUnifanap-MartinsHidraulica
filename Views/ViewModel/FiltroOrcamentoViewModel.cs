using MartinsHidraulica.Models;

namespace MartinsHidraulica.Views.ViewModel;

public class FiltroOrcamentoViewModel
{
    public string? Identificacao { get; set; }
    public string? NomeCliente { get; set; }
    public DateTimeOffset? DataInicial { get; set; }
    public DateTimeOffset? DataFinal { get; set; }
    public List<Orcamento>? Orcamentos { get; set; }
   
}