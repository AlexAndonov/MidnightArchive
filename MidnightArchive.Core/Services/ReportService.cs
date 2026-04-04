using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.ReportDTOs;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Enums;
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

		public async Task<ReportOperationResult> CreateAsync(ReportCreateDto model, string userId)
		{
			bool storyExists = await context.Stories
				.AnyAsync(s => s.Id == model.StoryId && !s.IsDeleted);

			if (!storyExists)
				return ReportOperationResult.StoryNotFound;

			bool alreadyReported = await context.Reports
				.AnyAsync(r => r.StoryId == model.StoryId && r.ReporterId == userId && !r.IsResolved);

			if (alreadyReported)
				return ReportOperationResult.AlreadyReported;

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

			return ReportOperationResult.Success;
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

		public async Task<ReportOperationResult> ResolveAsync(Guid reportId)
		{
			Report? report = await context.Reports
				.FirstOrDefaultAsync(r => r.Id == reportId);

			if (report == null)
				return ReportOperationResult.ReportNotFound;

			report.IsResolved = true;

			await context.SaveChangesAsync();

			return ReportOperationResult.Success;
		}
	}
}
