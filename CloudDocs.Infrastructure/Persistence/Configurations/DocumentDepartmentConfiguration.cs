using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudDocs.Infrastructure.Persistence.Configurations;

public class DocumentDepartmentConfiguration : IEntityTypeConfiguration<DocumentDepartment>
{
    public void Configure(EntityTypeBuilder<DocumentDepartment> builder)
    {
        builder.ToTable("DocumentDepartments");

        builder.HasKey(x => new { x.DocumentId, x.DepartmentId });

        builder.HasOne(x => x.Document)
            .WithMany(x => x.DocumentDepartments)
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Department)
            .WithMany(x => x.DocumentDepartments)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
