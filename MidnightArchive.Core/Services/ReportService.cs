using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.ReportDTOs;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Models;

namespace MidnightArchive.Core.Services
{
	public class ReportService : IReportService
	{
		private readonly ApplicationDbContext context;

		public ReportService(ApplicationDbContext _context)
		{
			context = _context;
		}

		public async Task CreateAsync(ReportCreateDto model, string userId)
		{
			bool storyExists = await context.Stories
				.AnyAsync(s => s.Id == model.StoryId && !s.IsDeleted);

			if (!storyExists)
			{
				throw new ArgumentException("Story does not exist.");
			}

			bool alreadyReported = await context.Reports
				.AnyAsync(r => r.StoryId == model.StoryId && r.ReporterId == userId && !r.IsResolved);

			if (alreadyReported)
			{
				throw new InvalidOperationException("You have already reported this story.");
			}

			Report report = new Report
			{
				StoryId = model.StoryId,
				ReporterId = userId,
				Reason = model.Reason,
				Description = model.Description,
				CreatedOn = DateTime.UtcNow,
				IsResolved = false
			};

			await context.Reports.AddAsync(report);
			await context.SaveChangesAsync();
		}

		public async Task<IEnumerable<ReportListDto>> GetAllAsync()
		{
			return await context.Reports
				.AsNoTracking()
				.OrderByDescending(r => r.CreatedOn)
				.Select(r => new ReportListDto
				{
					Id = r.Id,
					StoryId = r.StoryId,
					StoryTitle = r.Story.Title,
					ReporterName = r.Reporter.UserName!,
					Reason = r.Reason,
					Description = r.Description,
					CreatedOn = r.CreatedOn,
					IsResolved = r.IsResolved
				})
				.ToListAsync();
		}

		public async Task ResolveAsync(Guid reportId)
		{
			Report? report = await context.Reports
				.FirstOrDefaultAsync(r => r.Id == reportId);

			if (report == null)
			{
				throw new ArgumentException("Report not found.");
			}

			report.IsResolved = true;

			await context.SaveChangesAsync();
		}
	}
}
