using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Infra.Data.Models;

namespace MidnightArchive.Data
{
	public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
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


			builder.Entity<EventParticipant>()
				.HasKey(ep => new { ep.EventId, ep.ParticipantId });

			builder.Entity<EventParticipant>()
				.HasOne(ep => ep.Event)
				.WithMany(e => e.Participants)
				.HasForeignKey(ep => ep.EventId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.Entity<EventParticipant>()
				.HasOne(ep => ep.Participant)
				.WithMany(p => p.EventParticipants)
				.HasForeignKey(ep => ep.ParticipantId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.Entity<Report>()
				.HasOne(r => r.Story)
				.WithMany()
				.HasForeignKey(r => r.StoryId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.Entity<StoryLike>()
				.HasKey(sl => new { sl.StoryId, sl.UserId });

			builder.Entity<StoryLike>()
				.HasOne(sl => sl.Story)
				.WithMany(s => s.Likes)
				.HasForeignKey(sl => sl.StoryId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<StoryLike>()
				.HasOne(sl => sl.User)
				.WithMany()
				.HasForeignKey(sl => sl.UserId)
				.OnDelete(DeleteBehavior.Restrict);
		}

		public DbSet<Story> Stories { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Event> Events { get; set; }
		public DbSet<EventParticipant> EventParticipants { get; set; }
		public DbSet<Report> Reports { get; set; }
		public DbSet<StoryLike> StoryLikes { get; set; } = null!;
	}
}
