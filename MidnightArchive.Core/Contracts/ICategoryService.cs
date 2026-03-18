using MidnightArchive.Core.DTOs.CategoryDTOs;

namespace MidnightArchive.Core.Contracts
{
	public interface ICategoryService
	{
		Task<IEnumerable<CategoryListDto>> GetAllAsync();
		Task<CategoryDetailDto?> GetByIdAsync(int id);
		Task<CategoryDetailDto?> GetByIdAsync(int id, int page, int pageSize);
		Task<CategoryEditDto?> GetForEditAsync(int id);
		Task<CategoryDto> AddAsync(CategoryCreateDto model);
		Task<bool> EditAsync(CategoryEditDto model);
		Task<bool> SoftDeleteAsync(int id);
		Task<bool> HardDeleteAsync(int id);
	}
}
