using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.CommentDTOs;
using MidnightArchive.Core.Mappings;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Models;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;

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

		public async Task<bool> AddAsync(CommentCreateDto model, string userId)
		{
			var story = await context.Stories.FirstOrDefaultAsync(s => s.Id == model.StoryId && !s.IsDeleted);

			if (story == null)
				return false;

			Comment comment = new Comment()
			{
				Content = model.Content,
				CreatedOn = DateTime.UtcNow,
				AuthorId = userId,
				StoryId = model.StoryId,
			};

			await context.Comments.AddAsync(comment);
			if (await context.SaveChangesAsync() == 0)
			{
				return false;
			}
			

			return true;
		}

		public async Task<bool> HardDeleteAsync(Guid id, string userId)
		{
			Comment? comment = await context.Comments.FirstOrDefaultAsync(c => c.Id == id);

			if (comment == null)
				return false;

			if (userId != comment.AuthorId)
				return false;

			context.Comments.Remove(comment);
			if (await context.SaveChangesAsync() == 0)
				return false;

			return true;
		}

		public async Task<bool> EditAsync(CommentEditDto model, string userId)
		{
			Comment? comment = await context.Comments.FirstOrDefaultAsync(c => c.Id == model.Id);

			if (comment == null)
				return false;

			if (userId != comment.AuthorId)
				return false;

			comment.Content = model.Content;
			comment.ModifiedOn = DateTime.UtcNow;

			if (await context.SaveChangesAsync() == 0)
			{
				return false;
			}

			return true;
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
