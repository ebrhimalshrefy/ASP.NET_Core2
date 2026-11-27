using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Week5Api.Models;

namespace Week5Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Product table
            builder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
                entity.Property(p => p.Description).HasMaxLength(500);
                entity.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
                entity.Property(p => p.StockQuantity).IsRequired();
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
