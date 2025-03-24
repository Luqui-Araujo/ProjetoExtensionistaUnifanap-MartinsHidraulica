using DocumentFormat.OpenXml.Spreadsheet;
using iText.Kernel.Pdf;
using MartinsHidraulica.Data;
using MartinsHidraulica.Models;
using MartinsHidraulica.Views.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element; // Para elementos como parágrafos, tabelas, etc.
using iText.Layout.Properties;
using Border = iText.Layout.Borders.Border;
using Cell = iText.Layout.Element.Cell;
using Table = iText.Layout.Element.Table;

namespace MartinsHidraulica.Controllers;

public class ManterOrcamentosController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public ManterOrcamentosController
        (ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Orcamentos()
    {
        return View();
    }

    public IActionResult NovoOrcamento()
    {
        var empresa = _context.Empresas
            .FirstOrDefault();
        var tipoPagamento = _context.TiposPagamento
            .Where(t => t.Ativo == true)
            .ToList();
        var vendedores = _context.Vendedores
            .Where(t => t.Ativo == true)
            .ToList();

        var viewModel = new OrcamentoViewModel
        {
            EmpresaNome = empresa?.NomeEmpresa,
            EmpresaRazaoSocial = empresa?.RazaoSocial,
            EmpresaCnpj = empresa?.Cnpj,
            EmpresaCep = empresa?.Cep,
            EmpresaTelefone = empresa?.Telefone,
            EmpresaEmail = empresa?.Email,
            EmpresaEndereco = empresa?.Endereco,
            EmpresaComplemento = empresa?.Complemento,
            CondicoesPagamento = tipoPagamento,
            Vendedores = vendedores,
        };
        
        return View(viewModel);
    }
    
    //Gera o orçamento com os dados da View
    [HttpPost]
    public IActionResult GerarOrcamento(OrcamentoViewModel viewModel)
    {
        // Ensure Itens is not null to prevent errors
        if (viewModel.Itens == null)
            viewModel.Itens = new List<ItemOrcamento>();
        
        // Calculate totals safely
        decimal subtotal = viewModel.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);
    
        // Calcular desconto e acréscimo como percentuais
        decimal descontoPerc = Convert.ToDecimal(viewModel.Desconto);
        decimal acrescimoPerc = Convert.ToDecimal(viewModel.Acrescimo);

        decimal descontoValor = subtotal * (descontoPerc / 100);
        decimal acrescimoValor = subtotal * (acrescimoPerc / 100);
        decimal total = subtotal - descontoValor + acrescimoValor;
        
        var empresa = _context.Empresas.FirstOrDefault();
    
        var novoOrcamento = new Orcamento
        {
            ClienteId = viewModel.ClienteId,
            ClienteNome = viewModel.ClienteNome,
            ClienteTelefone = viewModel.ClienteTelefone,
            ClienteEmail = viewModel.ClienteEmail,
            ClienteCpfCnpj = viewModel.ClienteCpfCnpj,
            ClienteInscricaoEstadual = viewModel.ClienteInscricaoEstadual,
            ClienteCep = viewModel.ClienteCep,
            ClienteEndereco = viewModel.ClienteEndereco,
            ClienteEstado = viewModel.ClienteEstado,
            ClienteCidade = viewModel.ClienteCidade,
            ClienteBairro = viewModel.ClienteBairro,
            IdEmpresa = empresa?.Id ?? 0,
            DataOrcamento = DateTime.UtcNow,
            Subtotal = subtotal,
            Desconto = descontoValor,
            Acrescimo = acrescimoValor,
            Total = total,
            Vendedor = viewModel.Vendedor,
            CondicaoPagamento = viewModel.CondicaoPagamento, // Use string value, not collection
            Observacao = viewModel.Observacao,
            Identificacao = viewModel.Identificacao,
            Itens = viewModel.Itens
        };

        _context.Orcamentos.Add(novoOrcamento);
        _context.SaveChanges();

        return GerarPdfOrcamento(novoOrcamento.Id);
    }

    public IActionResult Download(int id)
    {
        return GerarPdfOrcamento(id);
    }

    private IActionResult GerarPdfOrcamento(int id)
    {
        //Pega os dados com as relações
        var orcamento = _context.Orcamentos
            .Include(o => o.Empresa)
            .FirstOrDefault(o => o.Id == id);

        if (orcamento == null)
        {
            return NotFound();
        }
        
        //Cria um memoryStream para salvar o PDF
        using (MemoryStream stream = new MemoryStream())
        {
            using (var writer = new PdfWriter(stream))
                using (var pdf = new PdfDocument(writer))
                using (var document = new Document(pdf, PageSize.A4, false))
                {
                    PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    
                    /*document.Add(new Paragraph($"ORÇAMENTO N° {orcamento.Id}").SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetFont(boldFont)
                        .SetMarginTop(-20)
                        .SetFontSize(14));*/
                    
                    //Informações da empresa
                    Table tableHeader = new Table(new float[] { 1, 3 })
                        .UseAllAvailableWidth()
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginBottom(10);

                    var empresa = _context.Empresas.FirstOrDefault();
                    if (empresa != null && empresa.LogoEmpresa != null)
                    {
                        ImageData imageData = ImageDataFactory.Create(empresa.LogoEmpresa);

                        Image logo = new Image(imageData).SetHeight(60).SetWidth(100).SetAutoScale(true)
                            .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                        
                        Cell imagemLogo = new Cell()
                            .Add(logo)
                            .SetTextAlignment(TextAlignment.CENTER) // Centraliza conteúdo na célula
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetMarginLeft(5)
                            .SetBorderRight(Border.NO_BORDER);
                        
                        tableHeader.AddCell(imagemLogo);
                    }
                    else
                    {
                        tableHeader.AddCell(new Cell().Add(new Paragraph("Logo não configurada")));
                    }
                    
                    
                    //Informações da empresa
                    Cell cell = new Cell()
                        .Add(new Paragraph(orcamento.Empresa.RazaoSocial).SetFontSize(12).SetFont(boldFont))
                        .Add(new Paragraph(orcamento.Empresa.Complemento ?? "").SetFontSize(8))
                        .Add(new Paragraph(orcamento.Empresa.Cep ?? "").SetFontSize(8))
                        .Add(new Paragraph($"Tel: {orcamento.Empresa.Telefone ?? ""}").SetFontSize(8))
                        /*.Add(new Paragraph($"Emitido em: {orcamento.DataOrcamento:dd/MM/yyyy}").SetFontSize(8))*/
                        .Add(new Paragraph($"E-mail: {orcamento.Empresa.Email ?? ""}").SetFontSize(8))
                        .SetBorderLeft(Border.NO_BORDER)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetPadding(10);
                    
                    tableHeader.AddCell(cell);
                    document.Add(tableHeader);
                    
                    
                    Table validadeOrcamento = new Table (new float[] { 1, 1 })
                        .UseAllAvailableWidth()
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginTop(5);
                    
                    validadeOrcamento.AddCell(new Paragraph($"Emitido em: {orcamento.DataOrcamento:dd/MM/yyyy}").SetFontSize(10));
                    validadeOrcamento.AddCell(new Paragraph($"Válido até: {orcamento.DataOrcamento.AddDays(5):dd/MM/yyyy}").SetFontSize(10));
                    
                    document.Add(validadeOrcamento);
                    
                    //Tabela cliente
                    
                    Table tableCliente = new Table(new float[] {1})
                        .UseAllAvailableWidth()
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginTop(5);

                    tableCliente.AddCell(new Paragraph("CLIENTE").SetFontSize(12).SetFont(boldFont))
                        .SetTextAlignment(TextAlignment.CENTER);
                    
                    document.Add(tableCliente);
                    
                    Table tableNomeTelefone = new Table(new float[] { 3, 1 })
                        .UseAllAvailableWidth()
                        .SetWidth(UnitValue.CreatePercentValue(100));
                    
                    tableNomeTelefone.AddCell(new Paragraph($"Nome: {orcamento.ClienteNome}").SetFontSize(10));
                    tableNomeTelefone.AddCell(new Paragraph($"Telefone: {orcamento.ClienteTelefone}").SetFontSize(10));
                    
                    document.Add(tableNomeTelefone);
                    
                    Table tableCpfCnpjInscricao = new Table(new float[] {2, 2})
                        .UseAllAvailableWidth()
                        .SetWidth(UnitValue.CreatePercentValue(100));
            
                    tableCpfCnpjInscricao.AddCell(new Paragraph($"CPF/CNPJ: {orcamento.ClienteCpfCnpj}").SetFontSize(10));
                    tableCpfCnpjInscricao.AddCell(new Paragraph($"Inscrição Estadual: {orcamento.ClienteInscricaoEstadual ?? ""}").SetFontSize(10));
            
                    document.Add(tableCpfCnpjInscricao);
            
                    Table tableCidadeEstadoBairro = new Table(new float[] {2, 2, 2, 1})
                        .UseAllAvailableWidth()
                        .SetWidth(UnitValue.CreatePercentValue(100));
            
                    tableCidadeEstadoBairro.AddCell(new Paragraph($"Cidade: {orcamento.ClienteCidade}").SetFontSize(10));
                    tableCidadeEstadoBairro.AddCell(new Paragraph($"Estado: {orcamento.ClienteEstado}").SetFontSize(10));
                    tableCidadeEstadoBairro.AddCell(new Paragraph($"Bairro: {orcamento.ClienteBairro}").SetFontSize(10));
                    tableCidadeEstadoBairro.AddCell(new Paragraph($"CEP: {orcamento.ClienteCep}").SetFontSize(10));
            
                    document.Add(tableCidadeEstadoBairro);
            
                    Table tableEndereco = new Table(new float[] {1})
                        .UseAllAvailableWidth()
                        .SetWidth(UnitValue.CreatePercentValue(100));
            
                    tableEndereco.AddCell(new Paragraph($"Endereço: {orcamento.ClienteEndereco}").SetFontSize(10));
            
                    document.Add(tableEndereco);
                    //Fim tabela cliente

                    var tableOrcamento = new Table(new float[] { 1 })
                        .UseAllAvailableWidth()
                        .SetWidth(UnitValue.CreatePercentValue(100));

                    tableOrcamento.AddCell(new Cell().Add(new Paragraph($"ORÇAMENTO N\u00b0: {orcamento.Id}")
                        .SetFont(boldFont).SetFontSize(12)))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginTop(5);

                    document.Add(tableOrcamento);
                    
                    //Tabela produto
                    var tableProdutos = new Table(new float[] {1, 5, 1, 1,})
                        .UseAllAvailableWidth()
                        .SetWidth(UnitValue.CreatePercentValue(100));
                    
                    tableProdutos.AddHeaderCell(new Cell().Add(new Paragraph("Quantidade").SetFont(boldFont).SetFontSize(10)));
                    tableProdutos.AddHeaderCell(new Cell().Add(new Paragraph($"Descrição").SetFont(boldFont).SetFontSize(10)));
                    tableProdutos.AddHeaderCell(new Cell().Add(new Paragraph("Valor Unitário").SetFont(boldFont).SetFontSize(10)));
                    tableProdutos.AddHeaderCell(new Cell().Add(new Paragraph("Total").SetFont(boldFont).SetFontSize(10)));
                    
                    foreach (var item in orcamento.Itens)
                    {
                        tableProdutos.AddCell(new Cell().Add(new Paragraph(item.Quantidade.ToString()))).SetFontSize(12);
                        tableProdutos.AddCell(new Cell().Add(new Paragraph($"{item.NomeProduto} \n {item.Descricao} "))).SetFontSize(10);
                        tableProdutos.AddCell(new Cell().Add(new Paragraph(item.PrecoUnitario.ToString("C")))).SetFontSize(12);
                        tableProdutos.AddCell(new Cell().Add(new Paragraph(item.Total.ToString("C")))).SetFontSize(12);
                    }
                    
                    document.Add(tableProdutos);
                    //Fim tabela produto
                    
                    //Tabela valores
                    int quantidadeTotalProdutos = orcamento.Itens.Sum(itens => itens.Quantidade);
                    var tabelaValores = new Table(new float[]{1, 2, 1, 1, 2})
                        .UseAllAvailableWidth()
                        .SetWidth(UnitValue.CreatePercentValue(100));

                    tabelaValores.AddCell(new Cell().Add(new Paragraph($"Total de itens: {quantidadeTotalProdutos}"))).SetFontSize(10);
                    tabelaValores.AddCell(new Cell().Add(new Paragraph($"Subtotal: {orcamento.Subtotal.ToString("C")}"))).SetFontSize(10);
                    tabelaValores.AddCell(new Cell().Add(new Paragraph($"Desconto: {orcamento.Desconto.ToString("C")}"))).SetFontSize(10);
                    tabelaValores.AddCell(new Cell().Add(new Paragraph($"Acréscimo: {orcamento.Acrescimo.ToString("C")}"))).SetFontSize(10);
                    tabelaValores.AddCell(new Cell().Add(new Paragraph($"Total: {orcamento.Total.ToString("C")}"))).SetFontSize(10);    
                    
                    document.Add(tabelaValores);
                    //Fim tabela valores
                    
                    //Tabela Vendedor
                    var tabelaVendedor = new Table(new float[]{1, 2})
                        .UseAllAvailableWidth()
                        .SetMarginTop(5)
                        .SetWidth(UnitValue.CreatePercentValue(100));
                    
                    tabelaVendedor.AddCell(new Cell().Add(new Paragraph($"Vendedor: {orcamento.Vendedor}")).SetFontSize(10));
                    tabelaVendedor.AddCell(new Paragraph($"Cond. Pagamento: {orcamento.CondicaoPagamento}").SetFontSize(10));
                    
                    document.Add(tabelaVendedor);
                    //Fim tabela vendedor
                    
                    //Tabela Observação

                    
                    var tabelaObservacao = new Table(new float[]{1})
                        .UseAllAvailableWidth()
                        .SetMarginTop(5)
                        .SetWidth(UnitValue.CreatePercentValue(100));
                    
                    tabelaObservacao.AddCell(new Cell().Add(new Paragraph("OBSERVAÇÃO").SetFontSize(12).SetFont(boldFont))
                        .SetTextAlignment(TextAlignment.CENTER))
                        .SetBorder(Border.NO_BORDER);
                    
                    tabelaObservacao.AddCell(new Cell().Add(new Paragraph(orcamento.Observacao ?? "Sem observações")).SetFontSize(10))
                        .SetBorderTop(Border.NO_BORDER);
                    
                    document.Add(tabelaObservacao);
                }
            //Retorna o arquivo PDF direto da memoryStream
            return File(stream.ToArray(), "application/pdf", $"Orcamento_{orcamento.Id}_{orcamento.ClienteNome.Replace(" ", "_")}.pdf");
        }
    }

    //Pesquisa de clientes e produtos
    [HttpGet]
    public JsonResult BuscarClientes(string termo)
    {
        if (string.IsNullOrEmpty(termo))
        {
            return Json(new List<object>());
        }
        
        var termoLower = termo.ToLower();
        
        var clientes = _context.Clientes
            .Where(c => c.Nome.ToLower().Contains(termoLower) || c.CpfCnpj.ToLower().Contains(termoLower) && c.Ativo == true)
            .Select(c => new
            {
                c.Id,
                c.Nome,
                c.CpfCnpj,
                c.InscricaoEstadual,
                c.Telefone,
                c.Email,
                c.Cep,
                c.Cidade,
                c.Estado,
                c.Bairro,
                c.Endereco
            })
            .ToList();
        
        return Json(clientes);
    }
    
    [HttpGet]
    public JsonResult BuscarProdutos(string termo)
    {
        if (string.IsNullOrEmpty(termo))
        {
            return Json(new List<object>());
        }
        
        var termoLower = termo.ToLower();
        
        var produtos = _context.Produtos
            .Where(p => p.Nome.ToLower().Contains(termoLower) && p.Ativo == true)
            .Select(p => new
            {
                p.Id,
                p.Nome,
                p.Descricao,
                p.Preco
            })
            .ToList();
        
        return Json(produtos);
    }
    
    
}