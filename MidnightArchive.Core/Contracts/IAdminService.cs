using MidnightArchive.Core.DTOs.ReportDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.Contracts
{
	public interface IAdminService
	{
		Task<IEnumerable<ReportedStoryAdminDto>> GetReportedStoriesAsync();
		Task ResolveReportsAsync(Guid storyId);
	}
}
