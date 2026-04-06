using MidnightArchive.Core.DTOs.Common;
using MidnightArchive.Core.DTOs.CategoryDTOs;

namespace MidnightArchive.Core.DTOs.StoryDTOs
{
	public class StoryIndexViewModel
	{
		public PagedResult<StorySummaryDto> Stories { get; set; } = new();

		public IEnumerable<CategoryListDto> Categories { get; set; } = new List<CategoryListDto>();

		public int? SelectedCategoryId { get; set; }
	}
}