using MidnightArchive.Infra.Data.Enums;
using System.ComponentModel.DataAnnotations;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Core.DTOs.ReportDTOs
{
	public class ReportCreateDto
	{
		[Required(ErrorMessage = FieldRequiredErrorMessage)]
		public Guid StoryId { get; set; }

		[Required(ErrorMessage = FieldRequiredErrorMessage)]
		public ReportReason Reason { get; set; }

		[MaxLength(ReportDescriptionMaxLength)]
		public string? Description { get; set; }
	}
}
