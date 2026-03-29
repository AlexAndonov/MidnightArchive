using System.ComponentModel.DataAnnotations;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Core.DTOs.EventDTOs
{
	public class EventEditDto
	{
		public Guid Id { get; set; }

		[Required(ErrorMessage = FieldRequiredErrorMessage)]
		[StringLength(EventTitleMaxLenth, MinimumLength = EventTitleMinLength)]
		public string Title { get; set; } = null!;


		[Required(ErrorMessage = FieldRequiredErrorMessage)]
		[StringLength(EventDescriptionMaxLength, MinimumLength = EventDescriptionMinLength)]
		public string Description { get; set; } = null!;


		public string? Location { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }
	}
}