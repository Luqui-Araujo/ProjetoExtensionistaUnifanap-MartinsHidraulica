using System.ComponentModel.DataAnnotations;

namespace MartinsHidraulica.Views.ViewModel;

public class ProdutoViewModel
{
    public int Id { get; set; }
    
    [Required]
    public string Nome { get; set; }
    public decimal Preco { get; set; }
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}