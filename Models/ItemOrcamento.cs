using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartinsHidraulica.Models;

public class ItemOrcamento
{
    [Key]
    public int Id { get; set; }
    public int OrcamentoId { get; set; }
    [ForeignKey("OrcamentoId")]
    public virtual Orcamento Orcamento { get; set; }
    public int? ProdutoId { get; set; }
    [ForeignKey("ProdutoId")]
    public virtual Produto Produto { get; set; }
    [Required]
    public string? NomeProduto { get; set; }
    [Required]
    public int Quantidade { get; set; }
    [Required]
    public decimal PrecoUnitario { get; set; }
    public string? Descricao { get; set; }
    public decimal Total => Quantidade * PrecoUnitario;
}