using System.ComponentModel.DataAnnotations;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Core.DTOs.CategoryDTOs
{
	public class CategoryCreateDto
	{
		[Required(ErrorMessage = RequiredErrorMessage)]
		[StringLength(CategoryTitleMaxLength, MinimumLength = CategoryTitleMinLength, ErrorMessage = RequiredLengthMessage)]
		public string Title { get; set; } = null!;

		[Required(ErrorMessage = RequiredErrorMessage)]
		[StringLength(CategoryDescriptionMaxLength, MinimumLength = CategoryDescriptionMinLength, ErrorMessage = RequiredLengthMessage)]
		public string Description { get; set; } = null!;
	}
}