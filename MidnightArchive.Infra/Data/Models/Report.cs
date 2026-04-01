using Microsoft.EntityFrameworkCore;
using MidnightArchive.Infra.Data.Enums;
using System.ComponentModel.DataAnnotations;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Infra.Data.Models
{
	[Comment("Report class")]
	public class Report
	{
		[Comment("Report identifier")]
		public Guid Id { get; set; }

		[Comment("Reported story identifier")]
		public Guid StoryId { get; set; }

		[Comment("Navigational property for the story")]
		public Story Story { get; set; } = null!;

		[Comment("Reporter identifier")]
		[Required]
		public string ReporterId { get; set; } = null!;

		[Comment("Navigational propery for the reporter")]
		[Required]
		public ApplicationUser Reporter { get; set; } = null!;

		[Comment("Reason for the report")]
		[Required]
		public ReportReason Reason { get; set; }

		[Comment("Optional description for the report")]
		[MaxLength(ReportDescriptionMaxLength)]
		public string? Description { get; set; }

		[Comment("Time of creation of report")]
		public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

		[Comment("Indicates whether report is resolved")]
		public bool IsResolved { get; set; }
	}
}
