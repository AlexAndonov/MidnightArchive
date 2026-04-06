using MidnightArchive.Core.DTOs.EventDTOs;
using MidnightArchive.Infra.Data.Enums;
using MidnightArchive.Infra.Data.Models;

namespace MidnightArchive.Core.Contracts
{
	public interface IEventService
	{
		Task<IEnumerable<EventListDto>> GetAllAsync();
		Task<EventDetailsDto?> GetByIdAsync(Guid id, string? userId);
		Task<EventEditDto?> GetByIdForEditAsync(Guid id);
		Task<EventDetailsDto> AddAsync(EventCreateDto model, string userId);
		Task<EventOperationResult> EditAsync(EventEditDto mode, string userId);
		Task<EventOperationResult> SoftDeleteAsync(Guid id, string userId);
		Task<EventOperationResult> HardDeleteAsync(Guid id, string userId);
		Task<bool> IsOwnerAsync(Guid eventId, string userId);
		Task<EventJoinResult> JoinAsync(Guid eventId, string userId);
		Task<EventLeaveResult> LeaveAsync(Guid eventId, string userId);
	}
}
