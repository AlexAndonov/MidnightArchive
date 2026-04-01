using MidnightArchive.Infra.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.DTOs.ReportDTOs
{
	public class ReportListDto
	{
		public Guid Id { get; set; }

		public Guid StoryId { get; set; }

		public string StoryTitle { get; set; } = null!;

		public string ReporterName { get; set; } = null!;

		public ReportReason Reason { get; set; }

		public string? Description { get; set; }

		public DateTime CreatedOn { get; set; }

		public bool IsResolved { get; set; }
	}
}
