using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.EventDTOs;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Enums;
using MidnightArchive.Infra.Data.Models;

namespace MidnightArchive.Core.Services
{
	public class EventService : IEventService
	{
		private readonly ApplicationDbContext context;
		private readonly IMapper mapper;

		public EventService(ApplicationDbContext _context, IMapper _mapper)
		{
			context = _context;
			mapper = _mapper;
		}

		public async Task<EventDetailsDto> AddAsync(EventCreateDto model, string userId)
		{
			if (model.EndDate <= model.StartDate)
				throw new ArgumentException("End date must be after Start date!");

			Event eventEntity = new Event()
			{
				Title = model.Title,
				Description = model.Description,
				Location = string.IsNullOrWhiteSpace(model.Location) ? "Online" : model.Location,
				StartDate = model.StartDate.Date,
				EndDate = model.EndDate.Date.AddDays(1).AddTicks(-1),
				CreatorId = userId
			};

			await context.Events.AddAsync(eventEntity);
			await context.SaveChangesAsync();

			return mapper.Map<EventDetailsDto>(eventEntity);
		}

		public async Task<EventOperationResult> SoftDeleteAsync(Guid id, string userId)
		{
			Event? eventEntity = await context.Events.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

			if (eventEntity == null)
				return EventOperationResult.NotFound;

			if (eventEntity.CreatorId != userId)
				return EventOperationResult.NotOwner;

			eventEntity.IsDeleted = true;
			await context.SaveChangesAsync();

			return EventOperationResult.Success;
		}

		public async Task<EventOperationResult> EditAsync(EventEditDto model, string userId)
		{
			Event? eventEntity = await context.Events.FirstOrDefaultAsync(e => e.Id == model.Id && !e.IsDeleted);

			if (eventEntity == null)
				return EventOperationResult.NotFound;

			if (eventEntity.CreatorId != userId)
				return EventOperationResult.NotOwner;

			if (model.EndDate <= model.StartDate)
				return EventOperationResult.InvalidDateRange;

			eventEntity.Title = model.Title;
			eventEntity.Description = model.Description;
			eventEntity.Location = string.IsNullOrWhiteSpace(model.Location) ? "Online" : model.Location;
			eventEntity.StartDate = model.StartDate.Date;
			eventEntity.EndDate = model.EndDate.Date.AddDays(1).AddTicks(-1);

			await context.SaveChangesAsync();

			return EventOperationResult.Success;
		}

		public async Task<IEnumerable<EventListDto>> GetAllAsync()
		{
			return await context.Events
				.AsNoTracking()
				.Where(e => !e.IsDeleted)
				.ProjectTo<EventListDto>(mapper.ConfigurationProvider)
				.ToListAsync();
		}

		public async Task<EventDetailsDto?> GetByIdAsync(Guid id, string? userId)
		{
			return await context.Events
				.AsNoTracking()
				.Where(e => e.Id == id && !e.IsDeleted)
				.Select(e => new EventDetailsDto
				{
					Id = e.Id,
					Title = e.Title,
					Description = e.Description,
					Location = e.Location,
					CreatorId = e.CreatorId,
					CreatorName = e.Creator.UserName!,
					StartDate = e.StartDate,
					EndDate = e.EndDate,
					ParticipantsCounts = e.Participants.Count,
					IsJoinedByCurrentUser = !string.IsNullOrWhiteSpace(userId)
						&& e.Participants.Any(p => p.ParticipantId == userId)
				})
				.FirstOrDefaultAsync();
		}

		public async Task<EventOperationResult> HardDeleteAsync(Guid id, string userId)
		{
			Event? eventEntity = await context.Events.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

			if (eventEntity == null)
				return EventOperationResult.NotFound;

			if (eventEntity.CreatorId != userId)
				return EventOperationResult.NotOwner;

			context.Events.Remove(eventEntity);
			await context.SaveChangesAsync();

			return EventOperationResult.Success;
		}

		public async Task<EventEditDto?> GetByIdForEditAsync(Guid id)
		{
			return await context.Events
				.Where(e => e.Id == id && !e.IsDeleted)
				.ProjectTo<EventEditDto>(mapper.ConfigurationProvider)
				.FirstOrDefaultAsync();
		}

		public async Task<bool> IsOwnerAsync(Guid eventId, string userId)
		{
			return await context.Events.AnyAsync(e => e.Id == eventId && e.CreatorId == userId && !e.IsDeleted);
		}

		public async Task<EventJoinResult> JoinAsync(Guid eventId, string userId)
		{
			Event? eventEntity = await context.Events
					.FirstOrDefaultAsync(e => e.Id == eventId && !e.IsDeleted);

			if (eventEntity == null)
			{
				return EventJoinResult.NotFound;
			}

			if (eventEntity.CreatorId == userId)
			{
				return EventJoinResult.OwnEvent;
			}

			if (eventEntity.EndDate <= DateTime.UtcNow)
			{
				return EventJoinResult.EventEnded;
			}

			bool alreadyJoined = await context.EventParticipants
				.AnyAsync(ep => ep.EventId == eventId && ep.ParticipantId == userId);

			if (alreadyJoined)
			{
				return EventJoinResult.AlreadyJoined;
			}

			EventParticipant participant = new EventParticipant()
			{
				EventId = eventId,
				ParticipantId = userId,
				JoinedOn = DateTime.UtcNow
			};

			await context.EventParticipants.AddAsync(participant);
			await context.SaveChangesAsync();

			return EventJoinResult.Success;
		}

		public async Task<EventLeaveResult> LeaveAsync(Guid eventId, string userId)
		{
			Event? eventEntity = await context.Events
				.FirstOrDefaultAsync(e => e.Id == eventId && !e.IsDeleted);

			if (eventEntity == null)
			{
				return EventLeaveResult.NotFound;
			}

			if (eventEntity.CreatorId == userId)
			{
				return EventLeaveResult.OwnEvent;
			}

			EventParticipant? participant = await context.EventParticipants
				.FirstOrDefaultAsync(ep => ep.EventId == eventId && ep.ParticipantId == userId);

			if (participant == null)
			{
				return EventLeaveResult.NotJoined;
			}

			context.EventParticipants.Remove(participant);
			await context.SaveChangesAsync();

			return EventLeaveResult.Success;
		}
	}
}
