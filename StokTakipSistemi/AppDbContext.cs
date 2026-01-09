using Microsoft.EntityFrameworkCore;

namespace StokTakipSistemi
{
    public class AppDbContext : DbContext
    {
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=StokTakip.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cascade Delete: Şube silinince ürünler gitsin
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Branch)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}