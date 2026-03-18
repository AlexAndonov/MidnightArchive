using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Constants;
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
		private readonly ICacheService cacheService;

		public StoryService(ApplicationDbContext _context, IMapper _mapper, ICacheService _cacheService)
		{
			context = _context;
			mapper = _mapper;
			cacheService = _cacheService;
        }

		public async Task<StoryDetailDto> AddAsync(StoryCreateDto model, string userId)
		{
			var user = await context.Users
					.Where(u => u.Id == userId)
					.Select(u => new { u.UserName })
					.FirstOrDefaultAsync();

			string? authorName = model.IsAnonymous ? null : user?.UserName ?? "Unknown";

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

            await cacheService.RemoveAsync(CacheKeys.StoriesAll);
            await cacheService.RemoveAsync(CacheKeys.StoriesByCategory(story.CategoryId));

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

            await cacheService.RemoveAsync(CacheKeys.StoriesAll);
            await cacheService.RemoveAsync(CacheKeys.StoriesByCategory(story.CategoryId));

            return true;
		}

		public async Task<IEnumerable<StorySummaryDto>> GetAllAsync()
		{
			var cacheKey = CacheKeys.StoriesAll;

			var cachedStories = await cacheService.GetAsync<IEnumerable<StorySummaryDto>>(cacheKey);

			if (cachedStories != null)
			{
				return cachedStories;
			}

            List<StorySummaryDto> stories = await context.Stories
					.AsNoTracking()
                    .Where(s => !s.IsDeleted)
					.ProjectTo<StorySummaryDto>(mapper.ConfigurationProvider)
					.ToListAsync();

			await cacheService.SetAsync(cacheKey, stories, TimeSpan.FromMinutes(10));

			return stories;
		}

		public async Task<StoryDetailDto?> GetByIdAsync(Guid id)
		{
            return await context.Stories
					.AsNoTracking()
					.Where(s => s.Id == id && !s.IsDeleted)
					.ProjectTo<StoryDetailDto>(mapper.ConfigurationProvider)
					.FirstOrDefaultAsync();
        }

		public async Task<StoryEditDto?> GetByIdForEditAsync(Guid id)
		{
            return await context.Stories
                   .AsNoTracking()
                   .Where(s => s.Id == id && !s.IsDeleted)
                   .ProjectTo<StoryEditDto>(mapper.ConfigurationProvider)
                   .FirstOrDefaultAsync();
        }

		public async Task<IEnumerable<StorySummaryDto>> GetStoriesByCategoryAsync(int categoryId)
		{
            var cacheKey = CacheKeys.StoriesByCategory(categoryId);

            var cachedStories = await cacheService.GetAsync<IEnumerable<StorySummaryDto>>(cacheKey);

            if (cachedStories != null)
            {
                return cachedStories;
            }

            var stories = await context.Stories
				.Where(s => !s.IsDeleted && s.CategoryId == categoryId)
				.Include(s => s.Author)
				.AsNoTracking()
				.ProjectTo<StorySummaryDto>(mapper.ConfigurationProvider)
				.ToListAsync();

            await cacheService.SetAsync(cacheKey, stories, TimeSpan.FromMinutes(10));

			return stories;
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

			await cacheService.RemoveAsync(CacheKeys.StoriesAll);
            await cacheService.RemoveAsync(CacheKeys.StoriesByCategory(story.CategoryId));

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

            await cacheService.RemoveAsync(CacheKeys.StoriesAll);
            await cacheService.RemoveAsync(CacheKeys.StoriesByCategory(story.CategoryId));

            return true;
		}
	}
}
