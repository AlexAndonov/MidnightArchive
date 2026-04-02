using MidnightArchive.Core.DTOs.CategoryDTOs;
using MidnightArchive.Core.DTOs.EventDTOs;
using MidnightArchive.Core.DTOs.StoryDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.DTOs.Home
{
	public class HomeIndexDto
	{
		public IEnumerable<StorySummaryDto> TopStoriesOfMonth { get; set; } = new List<StorySummaryDto>();
		public IEnumerable<StorySummaryDto> LatestStories { get; set; } = new List<StorySummaryDto>();
		public IEnumerable<CategoryListDto> FeaturedCategories { get; set; } = new List<CategoryListDto>();
		public IEnumerable<EventListDto> FeaturedEvents { get; set; } = new List<EventListDto>();
	}
}
