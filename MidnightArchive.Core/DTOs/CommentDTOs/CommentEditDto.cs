using System.ComponentModel.DataAnnotations;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Core.DTOs.CommentDTOs
{
	public class CommentEditDto
	{
		public Guid Id { get; set; }

		[Required(ErrorMessage = FieldRequiredErrorMessage)]
		[StringLength(CommentContentMaxLength, MinimumLength = CommentContentMinLength, ErrorMessage = RequiredLengthMessage)]
		public string Content { get; set; } = null!;
	}
}
