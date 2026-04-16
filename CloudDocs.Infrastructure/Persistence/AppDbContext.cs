using CloudDocs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CloudDocs.Infrastructure.Persistence;

/// <summary>
/// Provides access to the application database.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class
    /// using the specified options.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets the users.
    /// </summary>
    public DbSet<User> Users => Set<User>();
    /// <summary>
    /// Gets the roles.
    /// </summary>
    public DbSet<Role> Roles => Set<Role>();
    /// <summary>
    /// Gets the categories.
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();
    /// <summary>
    /// Gets the documents.
    /// </summary>
    public DbSet<Document> Documents => Set<Document>();
    /// <summary>
    /// Gets the document versions.
    /// </summary>
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    /// <summary>
    /// Gets the audit logs.
    /// </summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    /// <summary>
    /// Gets the password reset tokens.
    /// </summary>
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    /// <summary>
    /// Gets the refresh tokens.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    /// <summary>
    /// Gets the Document Types.
    /// </summary>
    public DbSet<DocumentTypeEntity> DocumentTypes => Set<DocumentTypeEntity>();

    /// <summary>
    /// Gets the Document Access Level.
    /// </summary>
    public DbSet<AccessLevelEntity> AccessLevels => Set<AccessLevelEntity>();

    /// <summary>
    /// Gets the Departments.
    /// </summary>
    public DbSet<Department> Departments => Set<Department>();

    /// <summary>
    /// Gets the Document departments.
    /// </summary>
    public DbSet<DocumentDepartment> DocumentDepartments => Set<DocumentDepartment>();

    /// <summary>
    /// Gets the set of clients in the context for querying and saving.
    /// </summary>
    public DbSet<Client> Clients => Set<Client>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}