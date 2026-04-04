using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Infra.Data.Enums
{
	public enum CommentOperationResult
	{
		Success = 1,
		NotFound = 2,
		NotTheAuthor = 3,
	}
}
