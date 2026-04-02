using Microsoft.AspNetCore.Identity;
using MidnightArchive.Infra.Data.Models;
using System.Runtime.InteropServices;

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

			if (!await roleManager.RoleExistsAsync(adminRole))
			{
				await roleManager.CreateAsync(new IdentityRole(adminRole));
			}

			var user = await userManager.FindByEmailAsync(adminEmail);

			if (user == null)
			{
				user = new ApplicationUser
				{
					UserName = adminEmail,
					Email = adminEmail
				};

				await userManager.CreateAsync(user, adminPassword);
			}

			if (!await userManager.IsInRoleAsync(user, adminRole))
			{
				await userManager.AddToRoleAsync(user, adminRole);
			}
		}
	}
}
