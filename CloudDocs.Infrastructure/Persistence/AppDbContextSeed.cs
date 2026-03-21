using CloudDocs.Domain.Entities;

namespace CloudDocs.Infrastructure.Persistence;

public static class AppDbContextSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (context.Users.Any())
            return;

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