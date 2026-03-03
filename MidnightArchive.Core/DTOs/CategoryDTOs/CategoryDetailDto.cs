using MidnightArchive.Core.DTOs.StoryDTOs;
using System.ComponentModel.DataAnnotations;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Core.DTOs.CategoryDTOs
{
	public class CategoryDetailDto
	{
		public int Id { get; set; }

		public string Title { get; set; } = null!;

		public string Description { get; set; } = null!;

		public ICollection<StorySummaryDto> Stories { get; set; } = new List<StorySummaryDto>();
	}
}
