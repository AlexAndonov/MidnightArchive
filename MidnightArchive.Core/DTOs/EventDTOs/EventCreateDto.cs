using System.ComponentModel.DataAnnotations;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Core.DTOs.EventDTOs
{
	public class EventCreateDto
	{

		[Required(ErrorMessage = FieldRequiredErrorMessage)]
		[StringLength(EventTitleMaxLenth, MinimumLength = EventTitleMinLength)]
		public string Title { get; set; } = null!;


		[Required(ErrorMessage = FieldRequiredErrorMessage)]
		[StringLength(EventDescriptionMaxLength, MinimumLength = EventDescriptionMinLength)]
		public string Description { get; set; } = null!;


		public string? Location { get; set; }


		[Required]
		public DateTime StartDate { get; set; }


		[Required]
		public DateTime EndDate { get; set; }
	}
}