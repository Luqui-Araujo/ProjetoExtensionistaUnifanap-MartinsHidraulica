using ClosedXML.Excel;            // para Excel
using DocumentFormat.OpenXml.Spreadsheet;
using iText.IO.Font.Constants;   // para PDF
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using MartinsHidraulica.Data;
using MartinsHidraulica.Helpers;
using MartinsHidraulica.Models;
using MartinsHidraulica.Views.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cell = iText.Layout.Element.Cell;
using Table = iText.Layout.Element.Table;

namespace MartinsHidraulica.Controllers;

public class ManterRelatoriosController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public ManterRelatoriosController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // GET página principla
    public IActionResult Relatorios()
    {
        ViewBag.Clientes = _context.Clientes
            .Where(c => c.Ativo)
            .Select(c => new { c.Id, c.Nome })
            .ToList();
        return View(new RelatoriosViewModel());
    }
    
    //POST
    
    //Post processa selecão de relatório e exibe priview
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Relatorios(RelatoriosViewModel vm, int page = 1)
    {
        const int pageSize = 10;
        dynamic result;

        switch (vm.TipoRelatorio)
        {
            case TipoRelatorio.Clientes:
                var clientesQ = _context.Clientes
                    .Where(c => c.Ativo)
                    .OrderBy(c => c.Id);

                result = Paginar(clientesQ.Select(c => new {
                    c.Id, c.Nome, c.CpfCnpj, c.Telefone
                }), pageSize, page);
                return PartialView("_RelatorioClientes", result);
            
            case TipoRelatorio.Produtos:
                var prodQ = _context.Produtos
                    .Where(p => p.Ativo)
                    .OrderBy(p => p.Id);
                result = Paginar(prodQ.Select(p => new {
                    p.Id, p.Nome, Preco = p.Preco.ToString("C")
                }), pageSize, page);
                return PartialView("_RelatorioProdutos", result);

            case TipoRelatorio.Orcamentos:
                var orcQ = _context.Orcamentos
                    .Include(o => o.Itens)
                    .AsQueryable();
                if (vm.DataInicial.HasValue)
                    orcQ = orcQ.Where(o => o.DataOrcamento >= vm.DataInicial.Value.Date.ToUniversalTime());
                if (vm.DataFinal  .HasValue)
                    orcQ = orcQ.Where(o => o.DataOrcamento <= vm.DataFinal .Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime());
                if (vm.ClienteId  .HasValue)
                    orcQ = orcQ.Where(o => o.ClienteId == vm.ClienteId.Value);
                orcQ = orcQ.OrderBy(o => o.Id);

                result = Paginar(orcQ.Select(o => new {
                    o.Id,
                    o.ClienteNome,
                    Data  = o.DataOrcamento.ToLocalTime().ToString("dd/MM/yyyy"),
                    o.Desconto,
                    o.Acrescimo,
                    Total = o.Total.ToString("C"),
                    o.CondicaoPagamento,
                    o.Vendedor,
                }), pageSize, page);
                return PartialView("_RelatorioOrcamentos", result);

            default:
                return BadRequest();
        }
    }
    
    // método genérico de paginação
    private PagedResult<TDto> Paginar<TDto>(IQueryable<TDto> query, int pageSize, int page)
    {
        var total = query.Count();
        var itens = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<TDto>
        {
            Items       = itens,
            CurrentPage = page,
            TotalPages  = (int)Math.Ceiling(total / (double)pageSize)
        };
    }
    
    //Exporta o resultado como PDF
    
    //PDF Orcamentos
    public IActionResult OrcamentosPdf(DateTime? dataInicial, DateTime? dataFinal, int? ClienteId)
    {
        var query = _context.Orcamentos.Include(o => o.Itens).Include(o => o.Empresa).AsQueryable();
        if (dataInicial.HasValue)
            query = query.Where(o => o.DataOrcamento >= dataInicial.Value.Date.ToUniversalTime());
        if (dataFinal.HasValue)
            query = query.Where(o => o.DataOrcamento <= dataFinal.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime());
        if (ClienteId.HasValue)
            query = query.Where(o => o.ClienteId == ClienteId.Value);

        var lista = query.ToList();
        
        using var ms = new MemoryStream();
        using var writer = new PdfWriter(ms);
        using var pdf = new PdfDocument(writer);
        using var doc = new Document(pdf, PageSize.A4.Rotate());
        var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        // cabeçalho, tabela etc. — monte como no GerarPdfOrcamento, mas iterando sobre "lista"
        doc.Add(new Paragraph("Relatório de Orçamentos")
            .SetFont(bold).SetFontSize(14).SetTextAlignment(TextAlignment.CENTER));

        var table = new Table(new float[] {1,1,1,1,1,1,1,1,1})
            .UseAllAvailableWidth();
        table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetFont(bold)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Cliente").SetFont(bold)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Data").SetFont(bold)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Desconto").SetFont(bold)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Acréscimo").SetFont(bold)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Total").SetFont(bold)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Quant Itens").SetFont(bold)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Cond Pag").SetFont(bold)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Vendedor").SetFont(bold)));

        foreach (var o in lista)
        {
            table.AddCell(o.Id.ToString());
            table.AddCell(o.ClienteNome);
            table.AddCell(o.DataOrcamento.ToLocalTime().ToString("dd/MM/yyyy"));
            table.AddCell(o.Desconto.ToString("C"));
            table.AddCell(o.Acrescimo.ToString("C"));
            table.AddCell(o.Total.ToString("C"));
            table.AddCell(o.Itens.Count.ToString());
            table.AddCell(o.CondicaoPagamento);
            table.AddCell(o.Vendedor);
        }

        doc.Add(table);
        doc.Close();

        return File(ms.ToArray(), "application/pdf", "Relatorio_Orcamentos.pdf");
    }
    
    // Excel Orçamentos
    public IActionResult OrcamentosExcel(DateTime? dataInicial, DateTime? dataFinal, int? clienteId)
    {
        // mesma query de filtro...
        var query = _context.Orcamentos
            .Include(o => o.Itens)
            .Include(o => o.Empresa)
            .AsQueryable();
        
        if (dataInicial.HasValue)
            query = query.Where(o => o.DataOrcamento >= dataInicial.Value.Date.ToUniversalTime());
        if (dataFinal.HasValue)
            query = query.Where(o => o.DataOrcamento <= dataFinal.Value.Date.AddDays(1).AddSeconds(-1).ToUniversalTime());
        if (clienteId.HasValue)
            query = query.Where(o => o.ClienteId == clienteId.Value);

        var lista = query.ToList();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Orçamentos");
        // cabeçalho
        ws.Cell(1, 1).Value = "ID";
        ws.Cell(1, 2).Value = "Cliente";
        ws.Cell(1, 3).Value = "Data";
        ws.Cell(1, 4).Value = "Desconto";
        ws.Cell(1, 5).Value = "Acréscimo";
        ws.Cell(1, 6).Value = "Total";
        ws.Cell(1, 7).Value = "Cond Pagamento";
        ws.Cell(1, 8).Value = "Vendedor";
        ws.Cell(1, 9).Value = "Quant Itens";
        ws.Cell(1, 10).Value = "Itens";
        ws.Cell(1, 11).Value = "Status";

        for (int i = 0; i < lista.Count; i++)
        {
            var o = lista[i];
            int row = i + 2;
            ws.Cell(row, 1).Value = o.Id;
            ws.Cell(row, 2).Value = o.ClienteNome;
            ws.Cell(row, 3).Value = o.DataOrcamento.ToLocalTime().ToString("dd/MM/yyyy");
            ws.Cell(row, 4).Value = o.Desconto;
            ws.Cell(row, 5).Value = o.Acrescimo;
            ws.Cell(row, 6).Value = o.Total;
            ws.Cell(row, 7).Value = o.CondicaoPagamento;
            ws.Cell(row, 8).Value = o.Vendedor;
            ws.Cell(row, 9).Value = o.Itens.Count;
            //Nome dos itens do orçamento
            var nomesItens = string.Join(", ", 
                o.Itens.Select(i => i.NomeProduto)
                );
            var cell = ws.Cell(row, 10).Value = nomesItens;
            ws.Cell(row, 10).Style.Alignment.WrapText = true;
            ws.Cell(row, 11).Value = o.Aprovado == true ? "Aprovado" : "Desaprovado";
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Relatorio_Orcamentos.xlsx");
    }
    
    // =============  CLIENTES  =============
public IActionResult ClientesPdf()
{
    var lista = _context.Clientes
                .Where(c => c.Ativo)
                .OrderBy(c => c.Id)
                .ToList();

    using var ms = new MemoryStream();
    using var pdfWriter = new PdfWriter(ms);
    using var pdf       = new PdfDocument(pdfWriter);
    using var doc       = new Document(pdf, PageSize.A4);
    var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

    doc.Add(new Paragraph("Relatório de Clientes")
            .SetFont(bold).SetFontSize(14)
            .SetTextAlignment(TextAlignment.CENTER));

    var table = new Table(new float[] {1,4,3,3}).UseAllAvailableWidth();
    table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetFont(bold)));
    table.AddHeaderCell(new Cell().Add(new Paragraph("Nome").SetFont(bold)));
    table.AddHeaderCell(new Cell().Add(new Paragraph("CPF/CNPJ").SetFont(bold)));
    table.AddHeaderCell(new Cell().Add(new Paragraph("Telefone").SetFont(bold)));

    foreach (var c in lista)
    {
        table.AddCell(c.Id.ToString());
        table.AddCell(c.Nome);
        table.AddCell(c.CpfCnpj);
        table.AddCell(c.Telefone);
    }
    doc.Add(table);
    doc.Close();

    return File(ms.ToArray(), "application/pdf", "Relatorio_Clientes.pdf");
}

public IActionResult ClientesExcel()
{
    var lista = _context.Clientes
                .Where(c => c.Ativo)
                .OrderBy(c => c.Id)
                .ToList();

    using var wb = new XLWorkbook();
    var ws = wb.Worksheets.Add("Clientes");
    ws.Cell(1,1).Value = "ID";
    ws.Cell(1,2).Value = "Nome";
    ws.Cell(1,3).Value = "CPF/CNPJ";
    ws.Cell(1,4).Value = "Telefone";

    for (int i = 0; i < lista.Count; i++)
    {
        var r = i + 2;
        ws.Cell(r,1).Value = lista[i].Id;
        ws.Cell(r,2).Value = lista[i].Nome;
        ws.Cell(r,3).Value = lista[i].CpfCnpj;
        ws.Cell(r,4).Value = lista[i].Telefone;
    }

    using var ms = new MemoryStream();
    wb.SaveAs(ms);
    return File(ms.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "Relatorio_Clientes.xlsx");
}

// =============  PRODUTOS  =============
public IActionResult ProdutosPdf()
{
    var lista = _context.Produtos
                .Where(p => p.Ativo)
                .OrderBy(p => p.Id)
                .ToList();

    using var ms = new MemoryStream();
    using var pdfWriter = new PdfWriter(ms);
    using var pdf       = new PdfDocument(pdfWriter);
    using var doc       = new Document(pdf, PageSize.A4);
    var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

    doc.Add(new Paragraph("Relatório de Produtos")
            .SetFont(bold).SetFontSize(14)
            .SetTextAlignment(TextAlignment.CENTER));

    var table = new Table(new float[] {1,4,2}).UseAllAvailableWidth();
    table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetFont(bold)));
    table.AddHeaderCell(new Cell().Add(new Paragraph("Nome").SetFont(bold)));
    table.AddHeaderCell(new Cell().Add(new Paragraph("Preço").SetFont(bold)));

    foreach (var p in lista)
    {
        table.AddCell(p.Id.ToString());
        table.AddCell(p.Nome);
        table.AddCell(p.Preco.ToString("C"));
    }
    doc.Add(table);
    doc.Close();

    return File(ms.ToArray(), "application/pdf", "Relatorio_Produtos.pdf");
}

public IActionResult ProdutosExcel()
{
    var lista = _context.Produtos
                .Where(p => p.Ativo)
                .OrderBy(p => p.Id)
                .ToList();

    using var wb = new XLWorkbook();
    var ws = wb.Worksheets.Add("Produtos");
    ws.Cell(1,1).Value = "ID";
    ws.Cell(1,2).Value = "Nome";
    ws.Cell(1,3).Value = "Preço";

    for (int i = 0; i < lista.Count; i++)
    {
        int r = i + 2;
        ws.Cell(r,1).Value = lista[i].Id;
        ws.Cell(r,2).Value = lista[i].Nome;
        ws.Cell(r,3).Value = lista[i].Preco;
    }

    using var ms = new MemoryStream();
    wb.SaveAs(ms);
    return File(ms.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "Relatorio_Produtos.xlsx");
}

    
}