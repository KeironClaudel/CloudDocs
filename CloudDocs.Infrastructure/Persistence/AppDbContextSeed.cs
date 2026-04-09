using CloudDocs.Domain.Entities;

namespace CloudDocs.Infrastructure.Persistence;

/// <summary>
/// Represents app db context seed.
/// </summary>
public static class AppDbContextSeed
{
    /// <summary>
    /// Seeds the database initial data.
    /// </summary>
    public static async Task SeedAsync(AppDbContext context)
    {
        // ACCESS LEVELS
        if (!context.AccessLevels.Any())
        {
            var accessLevels = new List<AccessLevelEntity>
            {
                new()
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Code = "INTERNAL_PUBLIC",
                    Name = "Público interno",
                    Description = "Solo visible para usuarios autenticados.",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Code = "ADMIN_ONLY",
                    Name = "Solo administradores",
                    Description = "Solo visible para administradores",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Code = "OWNER_ONLY",
                    Name = "Solo propietario",
                    Description = "Solo visible para el propietario del documento",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Code = "DEPARTMENT_ONLY",
                    Name = "Solo departamento",
                    Description = "Solo visible para departamentos especificos",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.AccessLevels.AddRange(accessLevels);
            await context.SaveChangesAsync();
        }

        // USERS
        if (!context.Users.Any())
        {
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = "System Administrator",
                Email = "admin@clouddocs.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                RoleId = adminRoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }
    }
}