using System.ComponentModel.DataAnnotations;

namespace MidnightArchive.Core.DTOs.CommentDTOs
{
	public class CommentDto
	{
		public Guid Id { get; set; }

		public string Content { get; set; } = null!;

		public DateTime CreatedOn { get; set; }

		public DateTime? ModifiedOn { get; set; }

		public string AuthorName { get; set; } = null!;

		public string AuthorId { get; set; } = null!;

		public Guid StoryId { get; set; }
	}
}
