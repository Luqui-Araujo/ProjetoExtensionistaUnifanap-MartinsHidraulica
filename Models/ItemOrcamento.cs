using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartinsHidraulica.Models;

[NotMapped]
public class ItemOrcamento
{
    [Key]
    public string NomeProduto { get; set; }
    public int Quantidade { get; set; }
    public string? Descricao { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Total => Quantidade * PrecoUnitario;
}