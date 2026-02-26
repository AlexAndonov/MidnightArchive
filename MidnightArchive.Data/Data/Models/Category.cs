using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static MidnightArchive.Data.Data.Constants.ValidationConstants;

namespace MidnightArchive.Data.Data.Models;

[Comment("Category class")]
public class Category
{
    [Comment("Category identifier")]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(CategoryTitleMaxLength)]
    [Comment("Title of the category")]
    public string Title { get; set; } = null!;
    
    [Required]
    [MaxLength(CategoryDescriptionMaxLength)]
    [Comment("Description of the category")]
    public string Description { get; set; } = null!;

    [Comment("Collection of all stories in the category")]
    public ICollection<Story> Stories { get; set; } =  new List<Story>();   
}