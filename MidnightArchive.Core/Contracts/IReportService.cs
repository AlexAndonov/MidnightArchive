using MidnightArchive.Core.DTOs.ReportDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.Contracts
{
	public interface IReportService
	{
		Task CreateAsync(ReportCreateDto model, string userId);
		Task<IEnumerable<ReportListDto>> GetAllAsync();
		Task ResolveAsync(Guid reportId);
	}
}
