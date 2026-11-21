using Microsoft.EntityFrameworkCore;

namespace CrudPedido.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pedido>()
            .HasOne(pe => pe.Cliente)
            .WithMany()
            .HasForeignKey(pe => pe.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pedido>()
            .HasOne(pe => pe.Producto)
            .WithMany()
            .HasForeignKey(pe => pe.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
