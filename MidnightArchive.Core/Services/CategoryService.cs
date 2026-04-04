using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.CategoryDTOs;
using MidnightArchive.Core.DTOs.Common;
using MidnightArchive.Core.DTOs.StoryDTOs;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Enums;
using MidnightArchive.Infra.Data.Models;

namespace MidnightArchive.Core.Services
{
	public class CategoryService : ICategoryService
	{
		private readonly ApplicationDbContext context;
		private readonly IMapper mapper;

		public CategoryService(ApplicationDbContext _context, IMapper _mapper)
		{
			context = _context;
			mapper = _mapper;
		}

		public async Task<CategoryDto> AddAsync(CategoryCreateDto model)
		{
			Category category = new Category()
			{
				Title = model.Title,
				Description = model.Description,
			};

			await context.Categories.AddAsync(category);
			await context.SaveChangesAsync();

			return mapper.Map<CategoryDto>(category);
		}

		public async Task<CategoryOperationResult> SoftDeleteAsync(int id)
		{
			Category? category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

			if (category == null)
			{
				return CategoryOperationResult.NotFound;
			}

			category.IsDeleted = true;
			await context.SaveChangesAsync();

			return CategoryOperationResult.Success;
		}

		public async Task<CategoryOperationResult> EditAsync(CategoryEditDto model)
		{
			Category? category = await context.Categories.FirstOrDefaultAsync(c => c.Id == model.Id && !c.IsDeleted);

			if (category == null)
			{
				return CategoryOperationResult.NotFound;
			}

			category.Title = model.Title;
			category.Description = model.Description;

			await context.SaveChangesAsync();

			return CategoryOperationResult.Success;
		}

		public async Task<IEnumerable<CategoryListDto>> GetAllAsync()
		{
			return await context.Categories
				   .Where(c => !c.IsDeleted)
				   .Select(c => new CategoryListDto
				   {
					   Id = c.Id,
					   Title = c.Title,
					   Description = c.Description,
					   StoriesCount = c.Stories.Count(s => !s.IsDeleted)
				   })
				   .OrderByDescending(c => c.StoriesCount)
				   .AsNoTracking()
				   .ToListAsync();
		}

		public async Task<CategoryDetailDto?> GetByIdAsync(int id)
		{
			var category = await context.Categories
				.Where(c => c.Id == id && !c.IsDeleted)
				.AsNoTracking()
				.Include(c => c.Stories.Where(s => !s.IsDeleted))
				.FirstOrDefaultAsync();

			if (category == null) return null;

			return new CategoryDetailDto
			{
				Id = category.Id,
				Title = category.Title,
				Description = category.Description,
				Stories = new PagedResult<StorySummaryDto>
				{
					Items = category.Stories
						.Where(s => !s.IsDeleted)
						.OrderByDescending(s => s.CreatedOn)
						.Select(s => new StorySummaryDto
						{
							Id = s.Id,
							Title = s.Title,
							Preview = s.Content.Length > 100 ? s.Content.Substring(0, 100) + "..." : s.Content,
							CreatedOn = s.CreatedOn,
							AuthorName = s.Author?.UserName ?? string.Empty,
							ViewsCount = s.ViewsCount,
							LikesCount = s.LikesCount,
							IsAnonymous = s.IsAnonymous
						})
						.ToList(),
					TotalCount = category.Stories.Count(s => !s.IsDeleted),
					Page = 1,
					PageSize = category.Stories.Count(s => !s.IsDeleted)
				}
			};
		}

		public async Task<CategoryDetailDto?> GetByIdAsync(int id, int page, int pageSize)
		{
			if (page < 1)
			{
				page = 1;
			}

			if (pageSize < 1)
			{
				pageSize = 10;
			}

			var category = await context.Categories
				.Where(c => c.Id == id && !c.IsDeleted)
				.AsNoTracking()
				.Select(c => new
				{
					c.Id,
					c.Title,
					c.Description
				})
				.FirstOrDefaultAsync();

			if (category == null)
			{
				return null;
			}

			var storiesQuery = context.Stories
				.Where(s => s.CategoryId == id && !s.IsDeleted)
				.OrderByDescending(s => s.CreatedOn)
				.AsNoTracking();

			int totalStories = await storiesQuery.CountAsync();

			var stories = await storiesQuery
				.Select(s => new StorySummaryDto
				{
					Id = s.Id,
					Title = s.Title,
					Preview = s.Content.Length > 100
						? s.Content.Substring(0, 100) + "..."
						: s.Content,
					CreatedOn = s.CreatedOn,
					AuthorName = s.IsAnonymous ? "Anonymous" : s.Author != null ? s.Author.UserName : "Anonymous",
					ViewsCount = s.ViewsCount,
					LikesCount = s.LikesCount,
					IsAnonymous = s.IsAnonymous
				})
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new CategoryDetailDto
			{
				Id = category.Id,
				Title = category.Title,
				Description = category.Description,
				Stories = new PagedResult<StorySummaryDto>
				{
					Items = stories,
					TotalCount = totalStories,
					Page = page,
					PageSize = pageSize
				}
			};
		}

		public async Task<CategoryEditDto?> GetForEditAsync(int id)
		{
			var category = await context.Categories
				.Where(c => c.Id == id && !c.IsDeleted)
				.FirstOrDefaultAsync();

			if (category == null)
			{
				return null;
			}

			return mapper.Map<CategoryEditDto>(category);
		}

		public async Task<CategoryOperationResult> HardDeleteAsync(int id)
		{
			var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id);

			if (category == null)
			{
				return CategoryOperationResult.NotFound;
			}

			context.Categories.Remove(category);
			await context.SaveChangesAsync();

			return CategoryOperationResult.Success;
		}
	}
}
