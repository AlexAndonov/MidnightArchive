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
			CreateMap<Story, StoryDetailDto>();
			CreateMap<Story, StoryEditDto>();
			CreateMap<Story, StorySummaryDto>();
		}
	}
}
