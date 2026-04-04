using MidnightArchive.Core.DTOs.ReportDTOs;
using MidnightArchive.Infra.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.Contracts
{
	public interface IReportService
	{
		Task<ReportOperationResult> CreateAsync(ReportCreateDto model, string userId);
		Task<IEnumerable<ReportListDto>> GetAllAsync();
		Task<ReportOperationResult> ResolveAsync(Guid reportId);
	}
}
