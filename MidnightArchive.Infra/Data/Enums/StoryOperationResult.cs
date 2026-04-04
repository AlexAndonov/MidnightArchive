using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Infra.Data.Enums
{
	public enum StoryOperationResult
	{
		Success = 0,
		NotFound = 1,
		AlreadyLiked = 2,
		LikeNotFound = 3,
	}
}
