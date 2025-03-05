using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartinsHidraulica.Models;

public class Departamentos
{
    [Key]
    public int IdDepartamento { get; set; }
    
    [Required]
    public string NomeDepartamento { get; set; }
    [Required]
    public int EmpresaId { get; set; }
    public bool Ativo { get; set; }
    [ForeignKey("EmpresaId")]
    public Empresa? Empresa { get; set; }
}