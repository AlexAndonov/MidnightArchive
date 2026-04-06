using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Infra.Data.Enums
{
	public enum EventLeaveResult
	{
		Success = 0,
		NotFound = 1,
		NotJoined = 2,
		OwnEvent = 3
	}
}
