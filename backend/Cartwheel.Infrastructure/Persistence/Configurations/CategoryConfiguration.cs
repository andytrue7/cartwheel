using Cartwheel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cartwheel.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        // Get-only Id: EF only maps properties with setters by convention, so name it explicitly.
        // The domain creates ids with Guid.NewGuid(), so the database never generates them.
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Name).HasMaxLength(100);

        // The domain doesn't enforce this yet; the database does.
        builder.HasIndex(c => c.Name).IsUnique();
    }
}
