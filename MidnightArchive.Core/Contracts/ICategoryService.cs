using MidnightArchive.Core.DTOs.CategoryDTOs;
using MidnightArchive.Infra.Data.Enums;

namespace MidnightArchive.Core.Contracts
{
	public interface ICategoryService
	{
		Task<IEnumerable<CategoryListDto>> GetAllAsync();
		Task<CategoryDetailDto?> GetByIdAsync(int id);
		Task<CategoryDetailDto?> GetByIdAsync(int id, int page, int pageSize);
		Task<CategoryEditDto?> GetForEditAsync(int id);
		Task<CategoryDto> AddAsync(CategoryCreateDto model);
		Task<CategoryOperationResult> EditAsync(CategoryEditDto model);
		Task<CategoryOperationResult> SoftDeleteAsync(int id);
		Task<CategoryOperationResult> HardDeleteAsync(int id);
	}
}
