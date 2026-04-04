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
				throw new ArgumentException("End Date must be after Start Date");

			Event eventEntity = new Event()
			{
				Title = model.Title,
				Description = model.Description,
				Location = model.Location ?? "Online",
				StartDate = model.StartDate,
				EndDate = model.EndDate,
				CreatorId = userId
			};

			await context.Events.AddAsync(eventEntity);
			await context.SaveChangesAsync();

			return mapper.Map<EventDetailsDto>(eventEntity);
		}

		public async Task<EventOperationResult> SoftDeleteAsync(Guid id)
		{
			Event? eventEntity = await context.Events.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

			if (eventEntity == null)
				return EventOperationResult.NotFound;

			eventEntity.IsDeleted = true;
			await context.SaveChangesAsync();

			return EventOperationResult.Success;
		}

		public async Task<EventOperationResult> EditAsync(EventEditDto model)
		{
			Event? eventEntity = await context.Events.FirstOrDefaultAsync(e => e.Id == model.Id && !e.IsDeleted);

			if (eventEntity == null)
				return EventOperationResult.NotFound;

			if (model.EndDate <= model.StartDate)
				throw new ArgumentException("End Date must be after Start Date");

			eventEntity.Title = model.Title;
			eventEntity.Description = model.Description;
			eventEntity.Location = model.Location ?? "Online";
			eventEntity.StartDate = model.StartDate;
			eventEntity.EndDate = model.EndDate;

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

		public async Task<EventDetailsDto?> GetByIdAsync(Guid id)
		{
			return await context.Events
				.AsNoTracking()
				.Where(e => e.Id == id && !e.IsDeleted)
				.ProjectTo<EventDetailsDto>(mapper.ConfigurationProvider)
				.FirstOrDefaultAsync();

		}

		public async Task<EventOperationResult> HardDeleteAsync(Guid id)
		{
			Event? eventEntity = await context.Events.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

			if (eventEntity == null)
				return EventOperationResult.NotFound;

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
	}
}
