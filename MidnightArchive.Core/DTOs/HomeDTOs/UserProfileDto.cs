using MidnightArchive.Core.DTOs.EventDTOs;
using MidnightArchive.Core.DTOs.ReportDTOs;
using MidnightArchive.Core.DTOs.StoryDTOs;

namespace MidnightArchive.Core.DTOs.HomeDTOs
{
	public class UserProfileDto
	{
		public string UserId { get; set; } = null!;
		public string UserName { get; set; } = null!;

		public IEnumerable<StorySummaryDto> Stories { get; set; } = new List<StorySummaryDto>();
		public IEnumerable<EventListDto> JoinedEvents { get; set; } = new List<EventListDto>();

		public IEnumerable<ReportedStoryDto> ReportedStories { get; set; } = new List<ReportedStoryDto>();
	}
}