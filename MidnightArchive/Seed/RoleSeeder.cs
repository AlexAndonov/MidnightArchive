using Microsoft.AspNetCore.Identity;
using MidnightArchive.Infra.Data.Models;

namespace MidnightArchive.Seed
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            const string adminRole = "Admin";

            const string adminEmail = "admin@midnight.com";
            const string adminPassword = "Admin123!";

            const string userEmail = "user@midnight.com";
            const string userPassword = "User123!";

            const string writerEmail = "writer@midnight.com";
            const string writerPassword = "Writer123!";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(admin, adminRole))
            {
                await userManager.AddToRoleAsync(admin, adminRole);
            }

            var user = await userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, userPassword);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create default user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            var writer = await userManager.FindByEmailAsync(writerEmail);
            if (writer == null)
            {
                writer = new ApplicationUser
                {
                    UserName = writerEmail,
                    Email = writerEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(writer, writerPassword);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create writer user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}