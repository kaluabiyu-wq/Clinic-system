using Clinic.Domain.Constants;
using Clinic.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Clinic.Infrastructure.Persistence.Seed;

public static class RoleAndAdminSeeder
{
    /// <summary>Call once at startup. Admin credentials come from config (Seed:AdminEmail / Seed:AdminPassword).</summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        foreach (var role in RoleNames.All)
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole<Guid>(role));

        var email = config["Seed:AdminEmail"];
        var password = config["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        if (await users.FindByEmailAsync(email) is not null) return;

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(), UserName = email, Email = email, EmailConfirmed = true,
            FullName = "System Administrator", IsActive = true
        };
        var created = await users.CreateAsync(admin, password);
        if (created.Succeeded) await users.AddToRoleAsync(admin, RoleNames.Administrator);
    }
}
