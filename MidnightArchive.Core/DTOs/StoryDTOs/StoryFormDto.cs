using System.ComponentModel.DataAnnotations;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Core.DTOs.StoryDTOs
{
	public class StoryFormDto
	{
		public Guid Id { get; set; }

		[Required(ErrorMessage = FieldRequiredErrorMessage)]
		[StringLength(StoryTitleMaxLength, MinimumLength = StoryTitleMinLength)]
		public string Title { get; set; } = null!;

		[Required(ErrorMessage = FieldRequiredErrorMessage)]
		[StringLength(StoryContentMaxLength, MinimumLength = StoryContentMinLength)]
		public string Content { get; set; } = null!;

		[Required(ErrorMessage = FieldRequiredErrorMessage)]
		public int CategoryId { get; set; }

		public bool IsAnonymous { get; set; }
	}
}