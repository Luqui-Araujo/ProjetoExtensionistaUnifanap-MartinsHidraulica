using MartinsHidraulica.Models;

namespace MartinsHidraulica.Views.ViewModel;

public class OrcamentoViewModel
{
    //Lista de empresas e clientes cadastrados
    public List<Empresa> Empresas { get; set; }
    public List<Cliente> Clientes { get; set; }
    
    //Informações da empresa
    public int EmpresaId { get; set; }
    public string EmpresaNome { get; set; }
    public string EmpresaRazaoSocial { get; set; }
    public string EmpresaCnpj { get; set; }
    public string EmpresaTelefone { get; set; }
    public string EmpresaEmail { get; set; }
    public string EmpresaCep { get; set; }
    public string EmpresaEndereco { get; set; }
    public string EmpresaComplemento { get; set; }
    
    
    //Informações do cliente
    public int? ClienteId { get; set; }
    public string ClienteNome { get; set; }
    public string? ClienteTelefone { get; set; }
    public string? ClienteEmail { get; set; }
    public string? ClienteCpfCnpj { get; set; }
    public string? ClienteInscricaoEstadual { get; set; }
    public string? ClienteCep { get; set; }
    public string? ClienteEndereco { get; set; }
    public string? ClienteCidade { get; set; }
    public string? ClienteBairro { get; set; }
    public string? ClienteEstado { get; set; }
    
    //Lista de vendedores e condições de pagamento
    public List<Vendedores> Vendedores { get; set; }
    public List<TiposPagamento> TiposPagamento { get; set; }
    
    //Selecionados
    public Cliente ClienteSelecionado { get; set; }
    public int EmpresaSelecionadaId { get; set; }
    
    //Lista de produtos
    public List<Produto> Produtos { get; set; }
    public List<ItemOrcamento> Itens { get; set; } = new List<ItemOrcamento>();
    
    //Valores
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal Acrescimo { get; set; }
    public decimal Total { get; set; }
    
    //Informações
    public string? Observacao { get; set; }
    public List <TiposPagamento> CondicaoPagamentos { get; set; }
    public string Vendedor { get; set; }
    public bool? Aprovado { get; set; }
    public string? Identificacao { get; set; }
    
}