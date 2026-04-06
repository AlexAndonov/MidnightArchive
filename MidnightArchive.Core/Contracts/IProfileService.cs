using MidnightArchive.Core.DTOs.HomeDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.Contracts
{
	public interface IProfileService
	{
		Task<UserProfileDto?> GetMyProfileAsync(string userId, bool isAdmin);
	}
}
