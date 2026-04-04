using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Infra.Data.Enums
{
	public enum ReportOperationResult
	{
		Success = 1,
		ReportNotFound = 2,
		AlreadyReported = 3,
		StoryNotFound = 4,
	}
}
