using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static MidnightArchive.Infra.Data.Constants.ValidationConstants;

namespace MidnightArchive.Infra.Data.Models
{
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

		[Comment("Indicates whether the category is soft-deleted")]
		public bool IsDeleted { get; set; }

		[Comment("Collection of all stories in the category")]
		public ICollection<Story> Stories { get; set; } = new List<Story>();
	}
}
