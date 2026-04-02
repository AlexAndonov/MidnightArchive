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

			builder.Services.AddStackExchangeRedisCache(options =>
			{
				options.Configuration = builder.Configuration["Redis:ConnectionString"];
				options.InstanceName = "MidnightArchive:";
			});

			var app = builder.Build();

			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				await RoleSeeder.SeedAsync(services);
			}

			if (app.Environment.IsDevelopment())
			{
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.MapStaticAssets();
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}")
				.WithStaticAssets();
			app.MapRazorPages()
			   .WithStaticAssets();

			app.Run();
		}
	}
}
