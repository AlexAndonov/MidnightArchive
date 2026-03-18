using AutoMapper;
using MidnightArchive.Core.DTOs.CommentDTOs;
using MidnightArchive.Infra.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.Mappings
{
	public class CommentProfile : Profile
	{
		public CommentProfile()
		{
			CreateMap<Comment, CommentDto>();
		}
	}
}
