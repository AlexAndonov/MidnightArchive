using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Constants;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.StoryDTOs;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Enums;
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

		public async Task<StoryOperationResult> EditAsync(StoryFormDto model)
		{
			Story? story = await context.Stories
				.FirstOrDefaultAsync(s => s.Id == model.Id && !s.IsDeleted);

			if (story == null)
			{
				return StoryOperationResult.NotFound;
			}

			int oldCategoryId = story.CategoryId;

			story.Title = model.Title;
			story.Content = model.Content;
			story.CategoryId = model.CategoryId;
			story.ModifiedOn = DateTime.UtcNow;

			await context.SaveChangesAsync();

			await cacheService.RemoveAsync(CacheKeys.StoriesAll);
			await cacheService.RemoveAsync(CacheKeys.StoriesByCategory(oldCategoryId));

			if (oldCategoryId != story.CategoryId)
			{
				await cacheService.RemoveAsync(CacheKeys.StoriesByCategory(story.CategoryId));
			}

			return StoryOperationResult.Success;
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

		public async Task<StoryDetailDto?> GetByIdAsync(Guid id, string? userId)
		{
			StoryDetailDto? story = await context.Stories
				.AsNoTracking()
				.Where(s => s.Id == id && !s.IsDeleted)
				.ProjectTo<StoryDetailDto>(mapper.ConfigurationProvider)
				.FirstOrDefaultAsync();

			if (story == null)
			{
				return null;
			}

			if (!string.IsNullOrEmpty(userId))
			{
				story.IsLikedByCurrentUser = await context.StoryLikes
					.AnyAsync(sl => sl.StoryId == id && sl.UserId == userId);
			}

			return story;
		}

		public async Task<StoryFormDto?> GetByIdForEditAsync(Guid id)
		{
			return await context.Stories
				.AsNoTracking()
				.Where(s => s.Id == id && !s.IsDeleted)
				.ProjectTo<StoryFormDto>(mapper.ConfigurationProvider)
				.FirstOrDefaultAsync();
		}

		public async Task<Guid?> GetRandomStoryIdAsync()
		{
			List<Guid> storyIds = await context.Stories
				.Where(s => !s.IsDeleted)
				.Select(s => s.Id)
				.ToListAsync();

			if (storyIds.Count == 0)
			{
				return null;
			}

			Random random = new Random();
			int randomIndex = random.Next(storyIds.Count);

			return storyIds[randomIndex];
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

		public async Task<StoryOperationResult> HardDeleteAsync(Guid id)
		{
			var story = await context.Stories
				.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

			if (story == null)
			{
				return StoryOperationResult.NotFound;
			}

			context.Remove(story);
			await context.SaveChangesAsync();

			await cacheService.RemoveAsync(CacheKeys.StoriesAll);
			await cacheService.RemoveAsync(CacheKeys.StoriesByCategory(story.CategoryId));

			return StoryOperationResult.Success;
		}

		public async Task<bool> HasUserLikedAsync(Guid storyId, string userId)
		{
			return await context.StoryLikes
				.AnyAsync(sl => sl.StoryId == storyId && sl.UserId == userId);
		}

		public async Task<StoryOperationResult> IncrementViewsAsync(Guid storyId)
		{
			Story? story = await context.Stories
				.FirstOrDefaultAsync(s => s.Id == storyId && !s.IsDeleted);

			if (story == null)
			{
				return StoryOperationResult.NotFound;
			}

			story.ViewsCount++;
			await context.SaveChangesAsync();

			return StoryOperationResult.Success;
		}

		public async Task<bool> IsAuthorAsync(Guid storyId, string userId)
		{
			return await context.Stories
				.AnyAsync(s => s.Id == storyId && s.AuthorId == userId && !s.IsDeleted);
		}

		public async Task<StoryOperationResult> LikeAsync(Guid storyId, string userId)
		{
			Story? story = await context.Stories
				.FirstOrDefaultAsync(s => s.Id == storyId && !s.IsDeleted);

			if (story == null)
			{
				return StoryOperationResult.NotFound;
			}

			bool alreadyLiked = await context.StoryLikes
				.AnyAsync(sl => sl.StoryId == storyId && sl.UserId == userId);

			if (alreadyLiked)
			{
				return StoryOperationResult.AlreadyLiked;
			}

			StoryLike storyLike = new StoryLike
			{
				StoryId = storyId,
				UserId = userId
			};

			await context.StoryLikes.AddAsync(storyLike);
			story.LikesCount++;

			await context.SaveChangesAsync();

			return StoryOperationResult.Success;
		}

		public async Task<StoryOperationResult> SoftDeleteAsync(Guid id)
		{
			var story = await context.Stories
				.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

			if (story == null)
			{
				return StoryOperationResult.NotFound;
			}

			story.IsDeleted = true;
			await context.SaveChangesAsync();

			await cacheService.RemoveAsync(CacheKeys.StoriesAll);
			await cacheService.RemoveAsync(CacheKeys.StoriesByCategory(story.CategoryId));

			return StoryOperationResult.Success;
		}

		public async Task<StoryOperationResult> UnlikeAsync(Guid storyId, string userId)
		{
			StoryLike? storyLike = await context.StoryLikes
				.FirstOrDefaultAsync(sl => sl.StoryId == storyId && sl.UserId == userId);

			if (storyLike == null)
			{
				return StoryOperationResult.LikeNotFound;
			}

			Story? story = await context.Stories
				.FirstOrDefaultAsync(s => s.Id == storyId && !s.IsDeleted);

			if (story == null)
			{
				return StoryOperationResult.NotFound;
			}

			context.StoryLikes.Remove(storyLike);

			if (story.LikesCount > 0)
			{
				story.LikesCount--;
			}

			await context.SaveChangesAsync();

			return StoryOperationResult.Success;
		}
	}
}