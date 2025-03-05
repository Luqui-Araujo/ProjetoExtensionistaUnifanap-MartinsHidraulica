namespace MartinsHidraulica.Views.ViewModel;

public class EmpresaViewModel
{
    public int Id { get; set; }
    public string NomeEmpresa { get; set; }
    public string RazaoSocial { get; set; }
    public string Cnpj { get; set; }
    public string Cep { get; set; }
    public string Endereco { get; set; }
    public string Complemento { get; set; }
    public string Telefone { get; set; }
    public string Email { get; set; }

    // Campo para receber o arquivo enviado pelo <input type="file">
    public IFormFile? LogoEmpresaFile { get; set; }
}