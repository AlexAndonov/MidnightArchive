using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.CommentDTOs;
using MidnightArchive.Infra.Data.Enums;
using System.Security.Claims;

namespace MidnightArchive.Controllers
{
	[Authorize]
	public class CommentController : Controller
	{
		private readonly ICommentService service;

		public CommentController(ICommentService _service)
		{
			service = _service;
		}

		[HttpPost]
		public async Task<IActionResult> Create(CommentCreateDto model)
		{
			if (!ModelState.IsValid)
				return RedirectToAction("Details", "Story", new { id = model.StoryId });

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(userId))
				return Unauthorized();


			CommentOperationResult result = await service.AddAsync(model, userId);

			if (result == CommentOperationResult.NotFound)
				return NotFound();


			return RedirectToAction("Details", "Story", new { id = model.StoryId });
		}

		[HttpPost]
		public async Task<IActionResult> Edit(CommentEditDto model)
		{
			if (!ModelState.IsValid)
				return RedirectToAction("Details", "Story", new { id = model.StoryId });

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(userId))
				return Unauthorized();

			CommentOperationResult result = await service.EditAsync(model, userId);

			switch (result)
			{
				case CommentOperationResult.Success:
					break;

				case CommentOperationResult.NotFound:
					return NotFound();

				case CommentOperationResult.NotTheAuthor:
					TempData["ErrorMessage"] = "You cannot edit this comment.";
					break;

				default:
					TempData["ErrorMessage"] = "Something went wrong.";
					break;
			}

			return RedirectToAction("Details", "Story", new { id = model.StoryId });
		}

		[HttpPost]
		public async Task<IActionResult> Delete(Guid id, Guid storyId)
		{
			if (id == Guid.Empty || storyId == Guid.Empty)
				return BadRequest();

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(userId))
				return Unauthorized();

			CommentOperationResult result = await service.HardDeleteAsync(id, userId);

			switch (result)
			{
				case CommentOperationResult.Success:
					break;

				case CommentOperationResult.NotFound:
					return NotFound();

				case CommentOperationResult.NotTheAuthor:
					TempData["ErrorMessage"] = "You can only delete your own comments.";
					break;
					
				default:
					TempData["ErrorMessage"] = "Something went wrong.";
					break;
			}

			return RedirectToAction("Details", "Story", new { id = storyId });
		}
	}
}
