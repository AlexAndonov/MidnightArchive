using MidnightArchive.Core.DTOs.StoryDTOs;
using System.Xml;

namespace MidnightArchive.Core.Contracts
{
	public interface IStoryService
	{
		Task<StoryDetailDto?> GetByIdAsync(Guid id);
		Task<StoryFormDto?> GetByIdForEditAsync(Guid id);
		Task<Guid?> GetRadnomStoryIdAsync();
		Task<IEnumerable<StorySummaryDto>> GetAllAsync();
		Task<IEnumerable<StorySummaryDto>> GetStoriesByCategoryAsync(int categoryId);
		Task<StoryDetailDto> AddAsync(StoryCreateDto model, string userId);
		Task<bool> EditAsync(StoryFormDto model);
		Task<bool> SoftDeleteAsync(Guid id);
		Task<bool> HardDeleteAsync(Guid id);
		Task<bool> IsAuthorAsync(Guid storyId, string userId);
		Task<bool> IncrementViewsAsync(Guid storyId);
	}
}
