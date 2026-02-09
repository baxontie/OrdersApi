using Microsoft.EntityFrameworkCore;
using OrdersApi.Models;

namespace OrdersApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.CustomerName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(o => o.Status)
                .IsRequired();

            entity.Property(o => o.Amount)
                .HasPrecision(18, 2);

            entity.Property(o => o.CreatedAt)
                .IsRequired();

            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => o.Amount);
            entity.HasIndex(o => o.CreatedAt);
        });
    }
}
