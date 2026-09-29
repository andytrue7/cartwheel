using Cartwheel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cartwheel.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // The database repeats the domain's rules, so bad data can't get in by any other route.
        builder.ToTable("Products", table =>
        {
            table.HasCheckConstraint("CK_Products_Price", "[Price] > 0");
            table.HasCheckConstraint(
                "CK_Products_StockQuantity",
                $"[StockQuantity] BETWEEN 0 AND {Product.MaxStockQuantity}");
        });

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        // Lengths match the request DTOs. Without them every string becomes nvarchar(max).
        builder.Property(p => p.Name).HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(2000);

        builder.Property(p => p.Price).HasPrecision(18, 2);

        // Product has no CategoryId property, so "CategoryId" is a shadow property: EF tracks
        // the column, the domain doesn't see it. Restrict overrides EF's default for a required
        // relationship, which is cascade: deleting a category must not delete its products.
        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey("CategoryId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
