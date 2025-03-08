using System.ComponentModel.DataAnnotations;

namespace MartinsHidraulica.Models;

public class Vendedores
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Nome { get; set; }
    public bool Ativo { get; set; }
}