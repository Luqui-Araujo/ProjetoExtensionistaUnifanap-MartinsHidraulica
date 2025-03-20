using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace MartinsHidraulica.Models;

public class Orcamento
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int? ClienteId { get; set; }
    [ForeignKey("ClienteId")]
    public virtual Cliente Cliente { get; set; }
    public string ClienteNome { get; set; }
    public string? ClienteTelefone { get; set; }
    public string? ClienteEmail { get; set; }
    public string? ClienteCpfCnpj { get; set; }
    public string? ClienteInscricaoEstadual { get; set; }
    public string? ClienteCep { get; set; }
    public string? ClienteEndereco { get; set; }
    public string? ClienteBairro { get; set; }
    public string? ClienteCidade { get; set; }
    public string? ClienteEstado { get; set; }
    
    public int IdEmpresa { get; set; }
    [ForeignKey("IdEmpresa")]
    public Empresa Empresa { get; set; }
    
    public DateTime DataOrcamento { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal Acrescimo { get; set; }
    public decimal Total { get; set; }
    public string Vendedor { get; set; }
    public string CondicaoPagamento { get; set; }
    public string? Observacao { get; set; }
    public bool? Aprovado { get; set; }
    public string? Identificacao { get; set; }
    public virtual ICollection<ItemOrcamento> Itens { get; set; } = new List<ItemOrcamento>();

   
 }