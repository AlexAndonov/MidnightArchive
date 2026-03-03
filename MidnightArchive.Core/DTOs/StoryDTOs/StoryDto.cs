using MidnightArchive.Core.DTOs.CommentDTOs;
using System.ComponentModel.DataAnnotations;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Core.DTOs.StoryDTOs
{
	public class StoryDto
	{
		public Guid Id { get; set; }

		public string Title { get; set; } = null!;

		public string Content { get; set; } = null!;


		public DateTime CreatedOn { get; set; }

		public DateTime? ModifiedOn { get; set; }

		[Required]
		public string AuthorId { get; set; } = null!;

		public int CategoryId { get; set; }

		public int ViewsCount { get; set; }

		public int LikesCount { get; set; }

		public bool IsAnonymous { get; set; }


		public ICollection<CommentDto> Comments { get; set; } = new List<CommentDto>();
	}
}