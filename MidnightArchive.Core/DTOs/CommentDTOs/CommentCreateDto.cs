using System.ComponentModel.DataAnnotations;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Core.DTOs.CommentDTOs
{
	public class CommentCreateDto
	{
		[Required(ErrorMessage = FieldRequiredErrorMessage)]
		[StringLength(CommentContentMaxLength, MinimumLength = CommentContentMinLength, ErrorMessage = RequiredLengthMessage)]
		public string Content { get; set; } = null!;

		[Required]
		public Guid StoryId { get; set; }
	}
}