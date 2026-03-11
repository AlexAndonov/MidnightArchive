using MidnightArchive.Core.DTOs.StoryDTOs;
using System.Xml;

namespace MidnightArchive.Core.Contracts
{
	public interface IStoryService
	{
		Task<StoryDetailDto?> GetByIdAsync(Guid id);
		Task<StoryEditDto?> GetByIdForEditAsync(Guid id); 
		Task<IEnumerable<StorySummaryDto>> GetAllAsync();
		Task<IEnumerable<StorySummaryDto>> GetStoriesByCategoryAsync(int categoryId);
		Task<StoryDetailDto> AddAsync(StoryCreateDto model, string userId);
		Task<bool> EditAsync(StoryEditDto model);
		Task<bool> SoftDeleteAsync(Guid id);
		Task<bool> HardDeleteAsync(Guid id);
	}
}
