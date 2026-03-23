using Microsoft.EntityFrameworkCore;
using FacturacionHN.Models;

namespace FacturacionHN.Data;

public class FacturacionDbContext : DbContext
{
    public FacturacionDbContext(DbContextOptions<FacturacionDbContext> options) : base(options) { }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<CAI> CAIs => Set<CAI>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<DetalleFactura> DetalleFacturas => Set<DetalleFactura>();
    public DbSet<CierreFacturacion> CierresFacturacion => Set<CierreFacturacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>()
            .HasIndex(e => e.RTN).IsUnique();

        // RTN único por empresa (no global)
        modelBuilder.Entity<Cliente>()
            .HasIndex(c => new { c.EmpresaId, c.RTN }).IsUnique();

        modelBuilder.Entity<Producto>()
            .HasIndex(p => new { p.EmpresaId, p.Codigo }).IsUnique();

        modelBuilder.Entity<CAI>()
            .HasIndex(c => new { c.EmpresaId, c.NumeroCai }).IsUnique();

        modelBuilder.Entity<Factura>()
            .HasIndex(f => f.NumeroFactura).IsUnique();

        // Relaciones Cliente -> Empresa
        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relaciones Producto -> Empresa
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Empresa)
            .WithMany()
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relaciones CAI -> Empresa
        modelBuilder.Entity<CAI>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relaciones Factura
        modelBuilder.Entity<Factura>()
            .HasOne(f => f.CAI)
            .WithMany(c => c.Facturas)
            .HasForeignKey(f => f.CAIId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Cliente)
            .WithMany(c => c.Facturas)
            .HasForeignKey(f => f.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Factura>()
            .HasOne(f => f.Empresa)
            .WithMany()
            .HasForeignKey(f => f.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relaciones Detalle
        modelBuilder.Entity<DetalleFactura>()
            .HasOne(d => d.Factura)
            .WithMany(f => f.Detalles)
            .HasForeignKey(d => d.FacturaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DetalleFactura>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CierreFacturacion>()
            .HasOne(c => c.Empresa)
            .WithMany()
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CierreFacturacion>()
            .HasIndex(c => new { c.EmpresaId, c.TipoCierre, c.FechaCierre }).IsUnique();
    }
}
