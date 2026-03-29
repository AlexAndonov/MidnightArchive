using AutoMapper;
using MidnightArchive.Core.DTOs.EventDTOs;
using MidnightArchive.Infra.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.Mappings
{
	public class EventProfile : Profile
	{
		public EventProfile()
		{
			CreateMap<Event, EventListDto>();
			CreateMap<Event, EventDetailsDto>();
			CreateMap<Event, EventEditDto>();
		}
	}
}
