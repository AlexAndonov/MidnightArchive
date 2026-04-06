using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.StoryDTOs;
using MidnightArchive.Infra.Data.Enums;
using System.Security.Claims;

namespace MidnightArchive.Controllers
{
	[Authorize]
	public class StoryController : Controller
	{
		private readonly IStoryService service;
		private readonly ICategoryService categoryService;

		public StoryController(IStoryService _service, ICategoryService _categoryService)
		{
			service = _service;
			categoryService = _categoryService;
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Index(int? categoryId)
		{
			IEnumerable<StorySummaryDto> stories;

			if (categoryId.HasValue && categoryId.Value <= 0)
			{
				return BadRequest();
			}

			if (categoryId.HasValue)
			{
				stories = await service.GetStoriesByCategoryAsync(categoryId.Value);
			}
			else
			{
				stories = await service.GetAllAsync();
			}

			return View(stories);
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Details(Guid id)
		{
			if (id == Guid.Empty)
			{
				return NotFound();
			}

			string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			StoryDetailDto? story = await service.GetByIdAsync(id, userId);

			if (story == null)
			{
				return NotFound();
			}

			string cookieKey = GetStoryViewCookieKey(id);

			bool alreadyViewed = Request.Cookies.ContainsKey(cookieKey);

			if (!alreadyViewed)
			{
				var viewResult = await service.IncrementViewsAsync(id);

				if (viewResult == StoryOperationResult.Success)
				{
					story.ViewsCount++;
				}

				CookieOptions options = new CookieOptions
				{
					HttpOnly = true,
					Expires = DateTimeOffset.UtcNow.AddHours(24),
					IsEssential = true,
					SameSite = SameSiteMode.Lax,
					Secure = Request.IsHttps
				};

				Response.Cookies.Append(cookieKey, "true", options);
			}

			return View(story);
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> Random()
		{
			Guid? storyId = await service.GetRandomStoryIdAsync();

			if (storyId == null)
			{
				return NotFound();
			}

			return RedirectToAction(nameof(Details), new { id = storyId.Value });
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			var categories = await categoryService.GetAllAsync();

			ViewBag.Categories = new SelectList(categories, "Id", "Title");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(StoryCreateDto model)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Categories = new SelectList(await categoryService.GetAllAsync(), "Id", "Title");
				return View(model);
			}

			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

			StoryDetailDto story = await service.AddAsync(model, userId);

			return RedirectToAction(nameof(Details), new { id = story.Id });
		}

		[HttpGet]
		public async Task<IActionResult> Edit(Guid id)
		{
			if (id == Guid.Empty)
			{
				return BadRequest();
			}

			StoryFormDto? story = await service.GetByIdForEditAsync(id);
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

			if (story == null)
			{
				return NotFound();
			}

			bool isAuthor = await service.IsAuthorAsync(story.Id, userId);

			if (!isAuthor)
			{
				return Forbid();
			}

			ViewBag.Categories = new SelectList(await categoryService.GetAllAsync(), "Id", "Title");

			return View(story);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(StoryFormDto model)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
			bool isAuthor = await service.IsAuthorAsync(model.Id, userId);

			if (!isAuthor)
			{
				return Forbid();
			}

			if (!ModelState.IsValid)
			{
				ViewBag.Categories = new SelectList(await categoryService.GetAllAsync(), "Id", "Title");
				return View(model);
			}

			StoryOperationResult result = await service.EditAsync(model);

			if (result == StoryOperationResult.NotFound)
			{
				return NotFound();
			}

			return RedirectToAction(nameof(Details), new { id = model.Id });
		}

		[HttpGet]
		public async Task<IActionResult> Delete(Guid id)
		{
			if (id == Guid.Empty)
			{
				return BadRequest();
			}

			StoryFormDto? story = await service.GetByIdForEditAsync(id);
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

			if (story == null)
			{
				return NotFound();
			}

			bool isAuthor = await service.IsAuthorAsync(story.Id, userId);
			bool isAdmin = User.IsInRole("Admin");

			if (!isAuthor && !isAdmin)
			{
				return Forbid();
			}

			ViewBag.Categories = new SelectList(await categoryService.GetAllAsync(), "Id", "Title");

			return View(story);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(Guid id)
		{
			if (id == Guid.Empty)
			{
				return BadRequest();
			}

			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
			bool isAuthor = await service.IsAuthorAsync(id, userId);
			bool isAdmin = User.IsInRole("Admin");

			if (!isAuthor && !isAdmin)
			{
				return Forbid();
			}

			StoryOperationResult result = await service.SoftDeleteAsync(id);

			if (result == StoryOperationResult.NotFound)
			{
				return NotFound();
			}

			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Like(Guid id)
		{
			if (id == Guid.Empty)
			{
				return BadRequest();
			}

			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

			StoryOperationResult result = await service.LikeAsync(id, userId);

			switch (result)
			{
				case StoryOperationResult.Success:
					return RedirectToAction(nameof(Details), new { id });

				case StoryOperationResult.NotFound:
					return NotFound();

				case StoryOperationResult.AlreadyLiked:
					TempData["WarningMessage"] = "You have already liked this story.";
					return RedirectToAction(nameof(Details), new { id });

				default:
					return BadRequest();
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Unlike(Guid id)
		{
			if (id == Guid.Empty)
			{
				return BadRequest();
			}

			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

			StoryOperationResult result = await service.UnlikeAsync(id, userId);

			switch (result)
			{
				case StoryOperationResult.Success:
					return RedirectToAction(nameof(Details), new { id });

				case StoryOperationResult.NotFound:
					return NotFound();

				case StoryOperationResult.LikeNotFound:
					TempData["WarningMessage"] = "You have not liked this story yet.";
					return RedirectToAction(nameof(Details), new { id });

				default:
					return BadRequest();
			}
		}

		private string GetStoryViewCookieKey(Guid storyId)
		{
			return $"story_viewed_{storyId}";
		}
	}
}