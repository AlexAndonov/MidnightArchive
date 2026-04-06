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

			string adminRole = "Admin";
			string adminEmail = "admin@midnight.com";
			string adminPassword = "Admin123!";

			string userEmail = "user@midnight.com";
			string userPassword = "User123!";

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
					Email = adminEmail
				};

				await userManager.CreateAsync(admin, adminPassword);
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
					Email = userEmail
				};

				await userManager.CreateAsync(user, userPassword);
			}
		}
	}
}