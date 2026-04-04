using MidnightArchive.Core.DTOs.CommentDTOs;
using MidnightArchive.Infra.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.Contracts
{
	public interface ICommentService
	{
		Task<IEnumerable<CommentDto>> GetAllForStoryAsync(Guid storyId);
		Task<CommentOperationResult> AddAsync(CommentCreateDto model, string userId);
		Task<CommentOperationResult> EditAsync(CommentEditDto model, string userId);
		Task<CommentOperationResult> HardDeleteAsync(Guid id, string userId);
	}
}
