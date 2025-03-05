using System.ComponentModel.DataAnnotations;

namespace MartinsHidraulica.Views.ViewModel;

public class ClienteViewModel
{
    public int Id { get; set; }
    [Required]
    public string Nome { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? CpfCnpj { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? Cep { get; set; }
    public string? Endereco { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public bool Ativo { get; set; }
    public string TipoCliente { get; set; }
}