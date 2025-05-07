using MartinsHidraulica.Models;

namespace MartinsHidraulica.Views.ViewModel;

public enum TipoRelatorio
{
    Clientes,
    Produtos,
    Orcamentos
}

public class RelatoriosViewModel
{
    public TipoRelatorio TipoRelatorio { get; set; }
    
    //Filtros
    public DateTime? DataInicial { get; set; }
    public DateTime? DataFinal { get; set; }
    public int? ClienteId { get; set; }
    
    //Resultados na preview
    public List<Cliente> ClientesData { get; set; }
    public List<Produto> ProdutosData { get; set; }
    public List<Orcamento> OrcamentosData { get; set; }
}