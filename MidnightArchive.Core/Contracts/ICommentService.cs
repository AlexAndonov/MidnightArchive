using MidnightArchive.Core.DTOs.CommentDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.Contracts
{
	public interface ICommentService
	{
		Task<IEnumerable<CommentDto>> GetAllForStoryAsync(Guid storyId);
		Task<bool> AddAsync(CommentCreateDto model, string userId);
		Task<bool> EditAsync(CommentEditDto model, string userId);
		Task<bool> HardDeleteAsync(Guid id, string userId);
	}
}
