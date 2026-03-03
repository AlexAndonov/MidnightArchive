using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Infra.Data.Models;

namespace MidnightArchive.Data
{
	public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
	{
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<Comment>()
				.HasOne(c => c.Story)
				.WithMany(c => c.Comments)
				.HasForeignKey(c => c.StoryId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Story>()
				.HasOne(s => s.Category)
				.WithMany(c => c.Stories)
				.HasForeignKey(s => s.CategoryId)
				.OnDelete(DeleteBehavior.Restrict);
		}

		public DbSet<Story> Stories { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Category> Categories { get; set; }
	}
}
