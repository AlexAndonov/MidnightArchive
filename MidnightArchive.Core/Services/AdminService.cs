using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.ReportDTOs;
using MidnightArchive.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.Services
{
	public class AdminService : IAdminService
	{
		private readonly ApplicationDbContext context;

		public AdminService(ApplicationDbContext _context)
		{
			context = _context;
		}

		public async Task<IEnumerable<ReportedStoryAdminDto>> GetReportedStoriesAsync()
		{
			return await context.Reports
				.AsNoTracking()
				.Where(r => !r.IsResolved && !r.Story.IsDeleted)
				.GroupBy(r => new
				{
					r.StoryId,
					r.Story.Title,
					r.Story.IsAnonymous,
					AuthorName = r.Story.Author.UserName
				})
				.Select(g => new ReportedStoryAdminDto
				{
					StoryId = g.Key.StoryId,
					StoryTitle = g.Key.Title,
					AuthorName = g.Key.IsAnonymous ? "Anonymous" : (g.Key.AuthorName ?? "Unknown"),
					ReportsCount = g.Count(),
					LatestReportDate = g.Max(r => r.CreatedOn),
					LatestReason = g
						.OrderByDescending(r => r.CreatedOn)
						.Select(r => r.Reason.ToString())
						.FirstOrDefault(),
					LatestDescription = g
						.OrderByDescending(r => r.CreatedOn)
						.Select(r => r.Description)
						.FirstOrDefault()
				})
				.OrderByDescending(r => r.LatestReportDate)
				.ToListAsync();
		}

		public async Task ResolveReportsAsync(Guid storyId)
		{
			var reports = await context.Reports
				.Where(r => r.StoryId == storyId && !r.IsResolved)
				.ToListAsync();

			foreach (var report in reports)
			{
				report.IsResolved = true;
			}

			await context.SaveChangesAsync();
		}
	}
}
