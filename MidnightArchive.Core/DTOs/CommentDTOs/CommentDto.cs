using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Core.DTOs.CommentDTOs
{
	public class CommentDto
	{
		public Guid Id { get; set; }

		[Required(ErrorMessage = FieldRequiredErrorMessage)]
		[StringLength(CommentContentMaxLength, MinimumLength = CommentContentMinLength, ErrorMessage = RequiredLengthMessage)]
		public string Content { get; set; } = null!;

		public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

		public DateTime? ModifiedOn { get; set; }

		[Required]
		public string AuthorId { get; set; } = null!;

		[Required]
		public Guid StoryId { get; set; }
	}
}
