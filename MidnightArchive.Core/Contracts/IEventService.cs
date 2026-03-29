using MidnightArchive.Core.DTOs.EventDTOs;
using MidnightArchive.Infra.Data.Models;

namespace MidnightArchive.Core.Contracts
{
	public interface IEventService
	{
		Task<IEnumerable<EventListDto>> GetAllAsync();
		Task<EventDetailsDto?> GetByIdAsync(Guid id);
		Task<EventEditDto?> GetByIdForEditAsync(Guid id);
		Task<EventDetailsDto> AddAsync(EventCreateDto model, string userId);
		Task<bool> EditAsync(EventEditDto model);
		Task<bool> SoftDeleteAsync(Guid id);
		Task<bool> HardDeleteAsync(Guid id);
	}
}
