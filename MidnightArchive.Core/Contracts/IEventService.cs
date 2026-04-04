using MidnightArchive.Core.DTOs.EventDTOs;
using MidnightArchive.Infra.Data.Enums;
using MidnightArchive.Infra.Data.Models;

namespace MidnightArchive.Core.Contracts
{
	public interface IEventService
	{
		Task<IEnumerable<EventListDto>> GetAllAsync();
		Task<EventDetailsDto?> GetByIdAsync(Guid id);
		Task<EventEditDto?> GetByIdForEditAsync(Guid id);
		Task<EventDetailsDto> AddAsync(EventCreateDto model, string userId);
		Task<EventOperationResult> EditAsync(EventEditDto model);
		Task<EventOperationResult> SoftDeleteAsync(Guid id);
		Task<EventOperationResult> HardDeleteAsync(Guid id);
	}
}
