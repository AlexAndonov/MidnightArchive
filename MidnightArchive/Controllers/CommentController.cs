using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.CommentDTOs;
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


			var success = await service.AddAsync(model, userId);

			if (!success)
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

			bool success = await service.EditAsync(model, userId);

			if (!success)
				return NotFound();

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

			bool success = await service.HardDeleteAsync(id, userId);

			if (!success)
				return NotFound();

			return RedirectToAction("Details", "Story", new { id = storyId });
		}
	}
}
