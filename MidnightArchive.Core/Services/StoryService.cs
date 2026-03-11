using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.StoryDTOs;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Models;

namespace MidnightArchive.Core.Services
{
	public class StoryService : IStoryService
	{
		private readonly ApplicationDbContext context;
		private readonly IMapper mapper;

		public StoryService(ApplicationDbContext _context, IMapper _mapper)
		{
			context = _context;
			mapper = _mapper;
		}

		public async Task<StoryDetailDto> AddAsync(StoryCreateDto model, string userId)
		{
			Story story = new Story()
			{
				Title = model.Title,
				Content = model.Content,
				CreatedOn = DateTime.UtcNow,
				AuthorId = userId,
				CategoryId = model.CategoryId,
				IsAnonymous = model.IsAnonymous,
				IsDeleted = false,
				Comments = new List<Comment>()
			};

			await context.AddAsync(story);
			await context.SaveChangesAsync();

			return mapper.Map<StoryDetailDto>(story);
		}

		public async Task<bool> EditAsync(StoryEditDto model)
		{
			Story? story = await context.Stories.FirstOrDefaultAsync(s => s.Id == model.Id);

			if (story == null)
			{
				return false;
			}

			story.Title = model.Title;
			story.Content = model.Content;
			story.CategoryId = model.CategoryId;
			story.ModifiedOn = DateTime.UtcNow;

			await context.SaveChangesAsync();

			return true;
		}

		public async Task<IEnumerable<StorySummaryDto>> GetAllAsync()
		{
			return await context.Stories
					.Where(s => !s.IsDeleted)
					.ProjectTo<StorySummaryDto>(mapper.ConfigurationProvider)
					.ToListAsync();
		}

		public async Task<StoryDetailDto?> GetByIdAsync(Guid id)
		{
			var story = await context.Stories.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

			if (story == null)
			{
				return null;
			}

			return mapper.Map<StoryDetailDto>(story);
		}

		public async Task<StoryEditDto?> GetByIdForEditAsync(Guid id)
		{
			var story = await context.Stories.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

			if (story == null)
			{
				return null;
			}

			return mapper.Map<StoryEditDto>(story);
		}

		public async Task<IEnumerable<StorySummaryDto>> GetStoriesByCategoryAsync(int categoryId)
		{
			return await context.Stories
				.Where(s => !s.IsDeleted && s.CategoryId == categoryId)
				.AsNoTracking()
				.ProjectTo<StorySummaryDto>(mapper.ConfigurationProvider)
				.ToListAsync();
		}

		public async Task<bool> HardDeleteAsync(Guid id)
		{
			var story = await context.Stories.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

			if (story == null)
			{
				return false;
			}

			context.Remove(story);
			await context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> SoftDeleteAsync(Guid id)
		{
			var story = await context.Stories.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

			if (story == null)
			{
				return false;
			}

			story.IsDeleted = true;
			await context.SaveChangesAsync();

			return true;
		}
	}
}
