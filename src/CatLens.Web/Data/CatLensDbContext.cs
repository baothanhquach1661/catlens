using CatLens.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CatLens.Web.Data;

public class CatLensDbContext : DbContext
{
    public CatLensDbContext(DbContextOptions<CatLensDbContext> options)
        : base(options)
    {
    }

    public DbSet<Variant> Variants => Set<Variant>();

    public DbSet<InventoryLevel> InventoryLevels => Set<InventoryLevel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Variant>(entity =>
        {
            entity.HasKey(variant => variant.Id);

            entity.Property(variant => variant.Handle)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(variant => variant.Title)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(variant => variant.Option1Value)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(variant => variant.Option2Value)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(variant => variant.Option3Value)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(variant => variant.Sku)
                .HasMaxLength(100);

            entity.HasIndex(variant => new
            {
                variant.Handle,
                variant.Option1Value,
                variant.Option2Value,
                variant.Option3Value
            })
                .IsUnique();

            entity.HasIndex(variant => variant.Sku);
        });

        modelBuilder.Entity<InventoryLevel>(entity =>
        {
            entity.HasKey(level => level.Id);

            entity.Property(level => level.Location)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(level => new
            {
                level.VariantId,
                level.Location
            })
                .IsUnique();

            entity.HasOne(level => level.Variant)
                .WithMany(variant => variant.InventoryLevels)
                .HasForeignKey(level => level.VariantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}