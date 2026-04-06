using Microsoft.EntityFrameworkCore;
using MidnightArchive.Data;

namespace MidnightArchive.Tests.Helpers
{
	public static class TestDbContextFactory
	{
		public static ApplicationDbContext Create()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

			return new ApplicationDbContext(options);
		}
	}
}