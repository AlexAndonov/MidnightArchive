using Microsoft.EntityFrameworkCore;
using MidnightArchive.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static MidnightArchive.Data.Data.Constants.ValidationConstants;

namespace MidnightArchive.Data.Data.Models
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

		[Comment("Indicates whether the comment is soft-deleted")]
		public bool IsDeleted { get; set; }

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
