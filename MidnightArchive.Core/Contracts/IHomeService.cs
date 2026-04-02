using MidnightArchive.Core.DTOs.CategoryDTOs;
using MidnightArchive.Core.DTOs.EventDTOs;
using MidnightArchive.Core.DTOs.Home;
using MidnightArchive.Core.DTOs.StoryDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.Contracts
{
	public interface IHomeService
	{
		Task<HomeIndexDto> GetHomePageDataAsync();
	}
}
