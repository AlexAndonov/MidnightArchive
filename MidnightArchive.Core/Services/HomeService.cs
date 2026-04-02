using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.CategoryDTOs;
using MidnightArchive.Core.DTOs.EventDTOs;
using MidnightArchive.Core.DTOs.Home;
using MidnightArchive.Core.DTOs.StoryDTOs;
using MidnightArchive.Data;

namespace MidnightArchive.Core.Services
{
	public class HomeService : IHomeService
	{
		private readonly ApplicationDbContext context;
		private readonly IMapper mapper;

		public HomeService(ApplicationDbContext context, IMapper mapper)
		{
			this.context = context;
			this.mapper = mapper;
		}

		public async Task<HomeIndexDto> GetHomePageDataAsync()
		{
			DateTime thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

			var topStories = await context.Stories
				.AsNoTracking()
				.Where(s => !s.IsDeleted && s.CreatedOn >= thirtyDaysAgo)
				.OrderByDescending(s => s.ViewsCount)
				.ThenByDescending(s => s.LikesCount)
				.Take(10)
				.ProjectTo<StorySummaryDto>(mapper.ConfigurationProvider)
				.ToListAsync();

			var latestStories = await context.Stories
				.AsNoTracking()
				.Where(s => !s.IsDeleted)
				.OrderByDescending(s => s.CreatedOn)
				.Take(10)
				.ProjectTo<StorySummaryDto>(mapper.ConfigurationProvider)
				.ToListAsync();

			var featuredCategories = await context.Categories
				.AsNoTracking()
				.Where(c => !c.IsDeleted)
				.OrderByDescending(c => c.Stories.Count(s => !s.IsDeleted))
				.Take(3)
				.ProjectTo<CategoryListDto>(mapper.ConfigurationProvider)
				.ToListAsync();

			var featuredEvents = await context.Events
				.AsNoTracking()
				.Where(e => !e.IsDeleted && e.StartDate > DateTime.UtcNow)
				.OrderBy(e => e.StartDate)
				.Take(3)
				.ProjectTo<EventListDto>(mapper.ConfigurationProvider)
				.ToListAsync();

			return new HomeIndexDto
			{
				TopStoriesOfMonth = topStories,
				LatestStories = latestStories,
				FeaturedCategories = featuredCategories,
				FeaturedEvents = featuredEvents
			};
		}
	}
}