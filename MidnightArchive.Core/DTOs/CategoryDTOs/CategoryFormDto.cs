using System.ComponentModel.DataAnnotations;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Core.DTOs.CategoryDTOs
{
    public class CategoryFormDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = FieldRequiredErrorMessage)]
        [StringLength(CategoryTitleMaxLength, MinimumLength = CategoryTitleMinLength, ErrorMessage = RequiredLengthMessage)]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = FieldRequiredErrorMessage)]
        [StringLength(CategoryDescriptionMaxLength, MinimumLength = CategoryDescriptionMinLength, ErrorMessage = RequiredLengthMessage)]
        public string Description { get; set; } = null!;
    }
}
