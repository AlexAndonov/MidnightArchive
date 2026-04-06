using AutoMapper;
using Microsoft.EntityFrameworkCore.Design.Internal;
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
			CreateMap<Event, EventListDto>()
				.ForMember(dest => dest.CreatorName,
					opt => opt.MapFrom(src => src.Creator.UserName))
				.ForMember(dest => dest.ParticipantsCount,
					opt => opt.MapFrom(src => src.Participants.Count));

			CreateMap<Event, EventDetailsDto>()
				.ForMember(dest => dest.CreatorName,
					opt => opt.MapFrom(src => src.Creator.UserName))
				.ForMember(dest => dest.ParticipantsCounts,
					opt => opt.MapFrom(src => src.Participants.Count))
				.ForMember(dest => dest.IsJoinedByCurrentUser,
					opt => opt.Ignore());

			CreateMap<Event, EventEditDto>();
		}
	}
}
