using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudDocs.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the persistence mapping for document.
/// </summary>
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    /// <summary>
    /// Configures the table Documents for EntityFramework
    /// </summary>
    /// <param name="builder">The builder.</param>
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.FileExtension).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Department).HasMaxLength(100);

        builder.HasIndex(x => x.OriginalFileName);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.Month);
        builder.HasIndex(x => x.Year);

        builder.HasIndex(x => x.DocumentType);
        builder.HasIndex(x => x.IsActive);

        builder.HasOne(x => x.Category)
            .WithMany(c => c.Documents)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UploadedByUser)
            .WithMany(u => u.UploadedDocuments)
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}