using Microsoft.EntityFrameworkCore;
using TradingService.Domain.Entities;

namespace TradingService.Infrastructure.Data;

public class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options)
{
    public DbSet<OrderEntity> Orders { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("DefaultConnection");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaseEntity>()
            .HasKey(entity => entity.Id);

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.Property(e => e.UserId)
                .IsRequired();
            entity.Property(e => e.StockTicker)
                .IsRequired()
                .HasMaxLength(10);
            entity.Property(e => e.PriceLimit)
                .IsRequired();
            entity.Property(e => e.Operation)
                .IsRequired();
            entity.Property(e => e.Amount)
                .IsRequired();
            entity.Property(e => e.IsCompleted)
                .IsRequired();
            entity.Property(e => e.IsActive)
                .IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}