using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Infra.Data.Models
{
	[Comment("Comment class")]
	public class Comment
	{
		[Comment("Comment identifier")]
		public Guid Id { get; set; }

		[Required]
		[MaxLength(CommentContentMaxLength)]
		[Comment("Text content of the comment")]
		public string Content { get; set; } = null!;

		[Comment("Date of creation of comment")]
		public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

		[Comment("Indicates the date when comment was edited")]
		public DateTime? ModifiedOn { get; set; }

		[Required]
		[Comment("Unique identifier of the comment's author")]
		public string AuthorId { get; set; } = null!;

		[ForeignKey(nameof(AuthorId))]
		[Comment("Navigation property for the comment's author")]
		public ApplicationUser Author { get; set; } = null!;

		[Required]
		[Comment("Unique identifier of the comment's story")]
		public Guid StoryId { get; set; }

		[ForeignKey(nameof(StoryId))]
		[Comment("Navigation property for the comment's story")]
		public Story Story { get; set; } = null!;
	}
}
