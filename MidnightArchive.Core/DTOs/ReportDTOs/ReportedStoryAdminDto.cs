using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.DTOs.ReportDTOs
{
	public class ReportedStoryAdminDto
	{
		public Guid StoryId { get; set; }

		public string StoryTitle { get; set; } = null!;

		public string AuthorName { get; set; } = null!;

		public int ReportsCount { get; set; }

		public DateTime LatestReportDate { get; set; }

		public string? LatestReason { get; set; }

		public string? LatestDescription { get; set; }
	}
}
