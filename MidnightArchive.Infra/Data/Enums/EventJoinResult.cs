using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Infra.Data.Enums
{
	public enum EventJoinResult
	{
		Success = 1,
		NotFound = 2,
		AlreadyJoined = 3,
		EventEnded = 4,
		OwnEvent = 5
	}
}
