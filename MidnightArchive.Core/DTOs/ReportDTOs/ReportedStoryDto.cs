using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.DTOs.ReportDTOs
{
	public class ReportedStoryDto
	{
		public Guid StoryId { get; set; }
		public string Title { get; set; } = null!;
		public string? AuthorName { get; set; }
		public int ReportsCount { get; set; }
		public DateTime LatestReportDate { get; set; }
	}
}
