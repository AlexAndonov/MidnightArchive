using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.DTOs.CategoryDTOs
{
	public class CategoryListDto
	{
		public int Id { get; set; }

		public string Title { get; set; } = null!;

		public string Description { get; set; } = null!;

		public int StoriesCount { get; set; }
	}
}
