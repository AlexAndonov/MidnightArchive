using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Data;
using MidnightArchive.Seed;

namespace MidnightArchive
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddApplicationServices();
			builder.Services.AddApplicationDbContext(builder.Configuration);
			builder.Services.AddApplicationIdentity(builder.Configuration);


			builder.Services.AddDatabaseDeveloperPageExceptionFilter();
			builder.Services.AddControllersWithViews();

			builder.Services.AddDistributedMemoryCache();

			var app = builder.Build();

			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;

				await RoleSeeder.SeedAsync(services);

				var context = services.GetRequiredService<ApplicationDbContext>();
				await DataSeeder.SeedAsync(context);
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Error/500");
				app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
				app.UseHsts();
			}
			else
			{
				app.UseDeveloperExceptionPage();
				app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
			}

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.MapRazorPages();

			app.Run();
		}
	}
}
