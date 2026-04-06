using AutoMapper;
using MidnightArchive.Core.DTOs.CategoryDTOs;
using MidnightArchive.Infra.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.Mappings
{
	public class CategoryProfile : Profile
	{
		public CategoryProfile()
		{
			CreateMap<Category, CategoryDto>();
			CreateMap<Category, CategoryEditDto>();
			CreateMap<Category, CategoryListDto>();
		}
	}
} 
