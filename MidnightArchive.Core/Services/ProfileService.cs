using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.EventDTOs;
using MidnightArchive.Core.DTOs.HomeDTOs;
using MidnightArchive.Core.DTOs.ReportDTOs;
using MidnightArchive.Core.DTOs.StoryDTOs;
using MidnightArchive.Data;

namespace MidnightArchive.Core.Services
{
	public class ProfileService : IProfileService
	{
		private readonly ApplicationDbContext context;

		public ProfileService(ApplicationDbContext _context)
		{
			context = _context;
		}

		public async Task<UserProfileDto?> GetMyProfileAsync(string userId, bool isAdmin)
		{
			UserProfileDto? profile = await context.Users
				.AsNoTracking()
				.Where(u => u.Id == userId)
				.Select(u => new UserProfileDto
				{
					UserId = u.Id,
					UserName = u.UserName!,
					Stories = u.Stories
						.Where(s => !s.IsDeleted)
						.OrderByDescending(s => s.CreatedOn)
						.Select(s => new StorySummaryDto
						{
							Id = s.Id,
							Title = s.Title,
							CreatedOn = s.CreatedOn,
						})
						.ToList(),

					JoinedEvents = u.EventParticipants
						.Where(ep => !ep.Event.IsDeleted)
						.OrderByDescending(ep => ep.JoinedOn)
						.Select(ep => new EventListDto
						{
							Id = ep.Event.Id,
							Title = ep.Event.Title,
							Location = ep.Event.Location,
							StartDate = ep.Event.StartDate,
							EndDate = ep.Event.EndDate
						})
						.ToList()
				})
				.FirstOrDefaultAsync();

			if (profile == null)
			{
				return null;
			}

			if (isAdmin)
			{
				profile.ReportedStories = await context.Reports
					.AsNoTracking()
					.Where(r => !r.IsResolved && !r.Story.IsDeleted)
					.GroupBy(r => new
					{
						r.StoryId,
						r.Story.Title,
						r.Story.IsAnonymous,
						AuthorName = r.Story.Author.UserName
					})
					.Select(g => new ReportedStoryDto
					{
						StoryId = g.Key.StoryId,
						Title = g.Key.Title,
						AuthorName = g.Key.IsAnonymous ? "Anonymous" : (g.Key.AuthorName ?? "Unknown"),
						ReportsCount = g.Count(),
						LatestReportDate = g.Max(r => r.CreatedOn)
					})
					.OrderByDescending(r => r.LatestReportDate)
					.ToListAsync();
			}

			return profile;
		}
	}
}