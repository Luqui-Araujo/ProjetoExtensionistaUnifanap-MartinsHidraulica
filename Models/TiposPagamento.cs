using System.ComponentModel.DataAnnotations;

namespace MartinsHidraulica.Models;

public class TiposPagamento
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "O nome do vendedor é obrigatório.")]
    public string Nome { get; set; }
    public bool Ativo { get; set; }
}