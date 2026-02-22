using Microsoft.EntityFrameworkCore;
using MidnightArchive.Data;
using MidnightArchive.Data.Models;
using System.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceExtensions
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services)
	{
		return services;
	}

	public static IServiceCollection AddApplicationIdentity(this IServiceCollection services)
	{
		services.AddDefaultIdentity<ApplicationUser>(options =>
		{
			options.SignIn.RequireConfirmedAccount = false;
		})
			.AddEntityFrameworkStores<ApplicationDbContext>();

		return services;
	}

	public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlServer(connectionString));

		return services;
	}
}
