using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.CategoryDTOs;
using MidnightArchive.Core.DTOs.CommentDTOs;
using MidnightArchive.Core.DTOs.StoryDTOs;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Models;
using System.Net.Mime;
using System.Runtime.InteropServices;

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

		public async Task<bool> SoftDeleteAsync(int id)
		{
			Category? category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id);

			if (category == null)
			{
				return false;
			}

			category.IsDeleted = true;
			await context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> EditAsync(CategoryEditDto model)
		{
			Category? category = await context.Categories.FirstOrDefaultAsync(c => c.Id == model.Id);

			if (category == null)
			{
				return false;
			}

			category.Title = model.Title;
			category.Description = model.Description;

			await context.SaveChangesAsync();

			return true;
		}

		public async Task<IEnumerable<CategoryListDto>> GetAllAsync()
		{
			var categories = await context.Categories
				.Where(c => !c.IsDeleted)     
				.AsNoTracking()
				.ToListAsync();

			return mapper.Map <IEnumerable<CategoryListDto>>(categories);
		}

		public async Task<CategoryDetailDto?> GetByIdAsync(int id)
		{
			var category = await context.Categories
				.Where(c => c.Id == id && !c.IsDeleted)
				.AsNoTracking()
				.Include(c => c.Stories.Where(s => !s.IsDeleted))
				.FirstOrDefaultAsync();

			if (category == null)
			{
				return null;
			}

			return mapper.Map<CategoryDetailDto>(category);
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

		public async Task<bool> HardDeleteAsync(int id)
		{
			var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id);

			if (category == null)
			{
				return false;
			}

			context.Categories.Remove(category);
			await context.SaveChangesAsync();

			return true;
		}
	}
}
