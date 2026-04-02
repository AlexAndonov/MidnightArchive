using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.Mappings;
using MidnightArchive.Core.Services;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Models;
using System.Configuration;
using ConfigurationManager = Microsoft.Extensions.Configuration.ConfigurationManager;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceExtensions
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			services.AddScoped<ICategoryService, CategoryService>();
			services.AddScoped<IStoryService, StoryService>();
			services.AddScoped<ICommentService, CommentService>();
			services.AddScoped<IEventService, EventService>();
			services.AddScoped<IHomeService, HomeService>();
			services.AddScoped<IReportService, ReportService>();
			services.AddScoped<ICacheService, RedisCacheService>();


			services.AddAutoMapper(cfg =>
			{
				cfg.AddProfile<CategoryProfile>();
				cfg.AddProfile<StoryProfile>();
				cfg.AddProfile<CommentProfile>();
				cfg.AddProfile<EventProfile>();
			});

			return services;
		}

		public static IServiceCollection AddApplicationIdentity(this IServiceCollection services, ConfigurationManager configuration)
		{
			services.AddDefaultIdentity<ApplicationUser>(options =>
			{
				ConfigureIdentity(options, configuration);
			})
				.AddRoles<IdentityRole>()
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

		private static void ConfigureIdentity(IdentityOptions options, ConfigurationManager configuration)
		{
			options.SignIn.RequireConfirmedAccount = configuration.GetValue<bool>("Identity:SignIn:RequireConfirmedAccount");

			options.SignIn.RequireConfirmedEmail = configuration.GetValue<bool>
				("Identity:SignIn:RequireConfirmedEmail");

			options.SignIn.RequireConfirmedPhoneNumber = configuration.GetValue<bool>
				("Identity:SignIn:RequireConfirmedPhoneNumber");

			options.Password.RequireDigit = configuration.GetValue<bool>
				("Identity:Password:RequireDigit");

			options.Password.RequiredLength = configuration.GetValue<int>
				("Identity:Password:RequiredLength");

			options.Password.RequiredUniqueChars = configuration.GetValue<int>
				("Identity:Password:RequiredUniqueChars");

			options.Password.RequireNonAlphanumeric = configuration.GetValue<bool>
				("Identity:Password:RequireNonAlphanumeric");

			options.Password.RequireUppercase = configuration.GetValue<bool>
				("Identity:Password:RequireUppercase");

			options.Password.RequireLowercase = configuration.GetValue<bool>
				("Identity:Password:RequireLowercase");

		}
	}
}
