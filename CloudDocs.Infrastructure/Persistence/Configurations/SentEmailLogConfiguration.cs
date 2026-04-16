using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudDocs.Infrastructure.Persistence.Configurations;

public class SentEmailLogConfiguration : IEntityTypeConfiguration<SentEmailLog>
{
    public void Configure(EntityTypeBuilder<SentEmailLog> builder)
    {
        builder.ToTable("SentEmailLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecipientEmail)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Subject)
            .HasMaxLength(250)
            .IsRequired();

        builder.HasOne(x => x.SentByUser)
            .WithMany()
            .HasForeignKey(x => x.SentByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Document)
            .WithMany()
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.SentByUserId);
        builder.HasIndex(x => x.DocumentId);
        builder.HasIndex(x => x.SentAt);
    }
}