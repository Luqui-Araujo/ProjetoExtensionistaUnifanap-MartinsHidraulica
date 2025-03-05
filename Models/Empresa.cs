using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace MartinsHidraulica.Models;

public class Empresa
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string NomeEmpresa { get; set; }
    public string RazaoSocial { get; set; }
    public string Cnpj { get; set; }
    public string Cep { get; set; }
    public string Endereco { get; set; }
    public string Complemento { get; set; }
    public string Telefone { get; set; }
    public string Email { get; set; }

    public byte[]? LogoEmpresa { get; set; } //Armazenar a imagem por blob
}