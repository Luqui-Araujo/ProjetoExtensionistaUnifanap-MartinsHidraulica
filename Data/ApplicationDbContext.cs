using MartinsHidraulica.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MartinsHidraulica.Data;

public class ApplicationDbContext : IdentityDbContext<Usuarios>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Departamentos> Departamentos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Vendedores> Vendedores { get; set; }
    public DbSet<TiposPagamento> TiposPagamento { get; set; }
}