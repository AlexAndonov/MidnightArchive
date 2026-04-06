using System.ComponentModel.DataAnnotations;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Core.DTOs.StoryDTOs
{
	public class StorySummaryDto
	{
		public Guid Id { get; set; }

		public string Title { get; set; } = null!;

		public string Preview { get; set; } = null!;

		public DateTime CreatedOn { get; set; }

		public string? AuthorName { get; set; }

		public int ViewsCount { get; set; }

		public int LikesCount { get; set; }

		public bool IsAnonymous { get; set; }
	}
}
