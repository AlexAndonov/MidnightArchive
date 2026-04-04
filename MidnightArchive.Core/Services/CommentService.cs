using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.CommentDTOs;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Enums;
using MidnightArchive.Infra.Data.Models;

namespace MidnightArchive.Core.Services
{
	public class CommentService : ICommentService
	{
		private readonly ApplicationDbContext context;
		private readonly IMapper mapper;
		public CommentService(ApplicationDbContext _context, IMapper _mapper)
		{
			context = _context;
			mapper = _mapper;
		}

		public async Task<CommentOperationResult> AddAsync(CommentCreateDto model, string userId)
		{
			Story? story = await context.Stories.FirstOrDefaultAsync(s => s.Id == model.StoryId && !s.IsDeleted);

			if (story == null)
				return CommentOperationResult.NotFound;

			Comment comment = new Comment()
			{
				Content = model.Content,
				CreatedOn = DateTime.UtcNow,
				AuthorId = userId,
				StoryId = model.StoryId,
			};

			await context.Comments.AddAsync(comment);
			await context.SaveChangesAsync();
			

			return CommentOperationResult.Success;
		}

		public async Task<CommentOperationResult> HardDeleteAsync(Guid id, string userId)
		{
			Comment? comment = await context.Comments.FirstOrDefaultAsync(c => c.Id == id);

			if (comment == null)
				return CommentOperationResult.NotFound;

			if (userId != comment.AuthorId)
				return CommentOperationResult.NotTheAuthor;

			context.Comments.Remove(comment);
			await context.SaveChangesAsync();

			return CommentOperationResult.Success;
		}

		public async Task<CommentOperationResult> EditAsync(CommentEditDto model, string userId)
		{
			Comment? comment = await context.Comments.FirstOrDefaultAsync(c => c.Id == model.Id);

			if (comment == null)
				return CommentOperationResult.NotFound;

			if (userId != comment.AuthorId)
				return CommentOperationResult.NotTheAuthor;

			comment.Content = model.Content;
			comment.ModifiedOn = DateTime.UtcNow;

			await context.SaveChangesAsync();

			return CommentOperationResult.Success;
		}

		public async Task<IEnumerable<CommentDto>> GetAllForStoryAsync(Guid storyId)
		{
			return await context.Comments
				.AsNoTracking()
				.Where(c => c.StoryId == storyId)
				.OrderByDescending(c => c.CreatedOn)
				.ProjectTo<CommentDto>(mapper.ConfigurationProvider)
				.ToListAsync();
		}
	}
}
