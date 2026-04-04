using MidnightArchive.Core.DTOs.StoryDTOs;
using MidnightArchive.Infra.Data.Enums;
using System.Xml;

namespace MidnightArchive.Core.Contracts
{
	public interface IStoryService
	{
		Task<StoryDetailDto?> GetByIdAsync(Guid id, string? userId);
		Task<StoryFormDto?> GetByIdForEditAsync(Guid id);
		Task<Guid?> GetRandomStoryIdAsync();
		Task<IEnumerable<StorySummaryDto>> GetAllAsync();
		Task<IEnumerable<StorySummaryDto>> GetStoriesByCategoryAsync(int categoryId);
		Task<StoryDetailDto> AddAsync(StoryCreateDto model, string userId);
		Task<StoryOperationResult> EditAsync(StoryFormDto model);
		Task<StoryOperationResult> SoftDeleteAsync(Guid id);
		Task<StoryOperationResult> HardDeleteAsync(Guid id);
		Task<bool> IsAuthorAsync(Guid storyId, string userId);
		Task<StoryOperationResult> IncrementViewsAsync(Guid storyId);
		Task<StoryOperationResult> LikeAsync(Guid storyId, string userId);
		Task<StoryOperationResult> UnlikeAsync(Guid storyId, string userId);
		Task<bool> HasUserLikedAsync(Guid storyId, string userId);
	}
}
