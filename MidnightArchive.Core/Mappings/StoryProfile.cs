using AutoMapper;
using MidnightArchive.Core.DTOs.StoryDTOs;
using MidnightArchive.Infra.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.Mappings
{
	public class StoryProfile : Profile
	{
		public StoryProfile()
		{
			CreateMap<Story, StoryDetailDto>()
				.ForMember(
					dest => dest.CategoryName,
					opt => opt.MapFrom(src => src.Category != null ? src.Category.Title : string.Empty)
				);

			CreateMap<Story, StoryFormDto>();
			CreateMap<Story, StorySummaryDto>()
						.ForMember(
							dest => dest.AuthorName,
							opt => opt.MapFrom(src => src.IsAnonymous ? null : src.Author.UserName)
						);
		}
	}
}
