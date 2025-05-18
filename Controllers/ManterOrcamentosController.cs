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
using iText.Layout;
using iText.Layout.Element; // Para elementos como parágrafos, tabelas, etc.
using iText.Layout.Properties;
using Border = iText.Layout.Borders.Border;
using Cell = iText.Layout.Element.Cell;
using Table = iText.Layout.Element.Table;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

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
    [ValidateAntiForgeryToken]
    public IActionResult GerarOrcamento(OrcamentoViewModel viewModel)
    {
        
        var empresa = _context.Empresas
            .FirstOrDefault();
        
        // Verifica se tem ao menos 1 item
        if (viewModel.Itens == null || !viewModel.Itens.Any())
        {
            return Json (new {success = false, message = "É necessário incluir ao menos um produto no orçamento."});
        }
        
        // Calculate totals safely
        decimal subtotal = viewModel.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);
    
        // Calcular desconto e acréscimo como percentuais
        decimal descontoPerc = Convert.ToDecimal(viewModel.Desconto);
        decimal acrescimoPerc = Convert.ToDecimal(viewModel.Acrescimo);

        decimal descontoValor = subtotal * (descontoPerc / 100);
        decimal acrescimoValor = subtotal * (acrescimoPerc / 100);
        decimal total = subtotal - descontoValor + acrescimoValor;
        
    
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
            DataOrcamento = DateTimeOffset.UtcNow,
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

        return Json(new { success = true, id = novoOrcamento.Id });
        
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
            .Include(o => o.Itens)
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

                        Image logo = new Image(imageData).SetHeight(60).SetWidth(160).SetAutoScale(true)
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
                        tableProdutos.AddCell(new Cell().Add(new Paragraph($"{item.NomeProduto} \n").SetFontSize(12)
                            .Add(new Paragraph($"{item.Descricao}").SetFontSize(10))));
                        tableProdutos.AddCell(new Cell().Add(new Paragraph(item.PrecoUnitario.ToString("C")))).SetFontSize(12);
                        tableProdutos.AddCell(new Cell().Add(new Paragraph(item.Total.ToString("C")))).SetFontSize(12);
                    }
                    
                    document.Add(tableProdutos);
                    //Fim tabela produto
                    
                    //Tabela valores
                    int quantidadeTotalProdutos = orcamento.Itens.Sum(itens => itens.Quantidade);
                    var tabelaValores = new Table(new float[]{1, 2, 1, 1, 2})
                        .UseAllAvailableWidth()
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginTop(5);

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

    //Obtem um orçamento específico
    [HttpGet]
    public IActionResult ObterOrcamento(int id)
    {
        
        var orcamento = _context.Orcamentos
            .Include(o => o.Itens)
            .FirstOrDefault(o => o.Id == id);

        if (orcamento == null)
        {
            return NotFound();
        }

        decimal subtotal = orcamento.Subtotal > 0 ? orcamento.Subtotal : 1;
        
        var dto = new
        {
            orcamento.Id,
            orcamento.Identificacao,
            orcamento.ClienteNome,
            orcamento.Vendedor,
            orcamento.CondicaoPagamento,
            orcamento.Observacao,
            
            Desconto = Math.Round((orcamento.Desconto / subtotal) * 100, 2),
            Acrescimo = Math.Round((orcamento.Acrescimo / subtotal) * 100, 2),
            
            orcamento.Subtotal,
            Itens = orcamento.Itens.Select(i => new
            {
                i.Id,
                i.ProdutoId,
                i.NomeProduto,
                i.Descricao,
                i.PrecoUnitario,
                i.Quantidade
            }).ToList()
        };

        return Json(dto);
    }
    
    //Carrega pro front as opções de vendedores e condições de pagamento
    [HttpGet]
    public JsonResult OpcoesVendedoresCondicoes()
    {
        var vendedores = _context.Vendedores
            .Where(v => v.Ativo)
            .Select(v => v.Nome)
            .ToList();

        var condicoes = _context.TiposPagamento
            .Where(c => c.Ativo)
            .Select(c => c.Nome)
            .ToList();

        return Json(new { vendedores, condicoes });
    }
    
    //Salva a edição de orçamento
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AtualizarOrcamento([FromBody] OrcamentoViewModel vm)
    {
        if (vm == null || vm.Id == 0)
        {
            return BadRequest("Dados inválidos recebidos.");
        }
        
        var orc = _context.Orcamentos
            .Include(o => o.Itens)
            .FirstOrDefault(o => o.Id == vm.Id);
        
        if (orc == null) return BadRequest("Orçamento não encontrado");

        // Atualiza campos simples
        orc.Identificacao = vm.Identificacao;
        orc.ClienteNome = vm.ClienteNome;
        orc.Vendedor = vm.Vendedor;
        orc.CondicaoPagamento = vm.CondicaoPagamento;
        orc.Observacao = vm.Observacao;
        
        // Sincroniza itens
        _context.ItemOrcamentos.RemoveRange(orc.Itens);          // jeito simples
        orc.Itens = vm.Itens;                                    // atribui a nova lista

        decimal subtotal = vm.Itens.Sum(i => i.Quantidade * i.PrecoUnitario);
        decimal descontoValor = subtotal * (vm.Desconto / 100);
        decimal acrescimoValor = subtotal * (vm.Acrescimo / 100);
        decimal total = subtotal - descontoValor + acrescimoValor;
        
        orc.Subtotal = subtotal;
        orc.Desconto = descontoValor;
        orc.Acrescimo = acrescimoValor;
        orc.Total = total;
        
        _context.SaveChanges();
        return Json(new { success = true, message = "Orçamento atualizado!" });
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
    
    //Listagem dos orçamentos cadastrados
    [HttpGet]
    public IActionResult Orcamentos(string? identificacao, string? nomeCliente, DateTime? dataInicial,
        DateTime? dataFinal)
    {
        var query = _context.Orcamentos.AsQueryable();

        if (!string.IsNullOrEmpty(identificacao))
        {
            query = query.Where(o => o.Identificacao != null && o.Identificacao.ToLower().Contains(identificacao.ToLower()));
        }

        if (!string.IsNullOrEmpty(nomeCliente))
        {
            query = query.Where(o => o.ClienteNome.ToLower().Contains(nomeCliente.ToLower()));
        }
        
        if (dataInicial.HasValue)
        {
            // Pega o início do dia
            DateTimeOffset dtIni = dataInicial.Value.Date.ToUniversalTime();
            query = query.Where(o => o.DataOrcamento >= dtIni);
        }

        if (dataFinal.HasValue)
        {
            // Pega o final do dia
            DateTimeOffset dtFim = dataFinal.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime();
            query = query.Where(o => o.DataOrcamento <= dtFim);
        }
        
        // Se nenhum filtro, pega os 10 últimos
        bool filtrosPreenchidos = 
            !string.IsNullOrEmpty(identificacao) ||
            !string.IsNullOrEmpty(nomeCliente) ||
            dataInicial.HasValue ||
            dataFinal.HasValue;
        
        if (!filtrosPreenchidos)
        {
            query = query.OrderByDescending(o => o.DataOrcamento).Take(10);
        }

        var viewModel = new FiltroOrcamentoViewModel
        {
            Identificacao = identificacao,
            NomeCliente = nomeCliente,
            DataInicial = dataInicial,
            DataFinal = dataFinal,
            Orcamentos = query
                .Include(o => o.Empresa)
                .ToList()
        };

        return View(viewModel);
    }
    
    //Botoes de aprovar e desaprovar orçamento
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Aprovar(int id)
    {
        var orcamento = _context.Orcamentos.FirstOrDefault(o => o.Id == id);
        if (orcamento != null)
        {
            orcamento.Aprovado = true;
            _context.SaveChanges();
            return Json(new { success = true, message = "Orcamento Aprovado com sucesso" });
        } 

        return BadRequest("Orcamento não encontrado!");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reprovar(int id)
    {
        var orcamento = _context.Orcamentos.FirstOrDefault(o => o.Id == id);
        if (orcamento != null)
        {
            orcamento.Aprovado = false;
            _context.SaveChanges();
            return Json(new { success = true, message = "Orcamento Desaprovado" });
        } 

        return BadRequest("Orcamento não encontrado!");
    }
}