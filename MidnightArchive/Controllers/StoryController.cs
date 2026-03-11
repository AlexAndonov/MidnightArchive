using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.StoryDTOs;
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

			if (categoryId <= 0)
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
				return BadRequest();
			}

			var story = await service.GetByIdAsync(id);

			if (story == null)
			{
				return NotFound();
			}

			return View(story);
		}


		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(StoryCreateDto model)
		{
			if (ModelState.IsValid == false)
			{
				return View(model);
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (userId == null)
			{
				return Unauthorized();
			}

			var story = await service.AddAsync(model, userId);

			return RedirectToAction(nameof(Details), new { Id = story.Id });
		}

		[HttpGet]
		public async Task<IActionResult> Edit(Guid id)
		{
			if (id == Guid.Empty)
			{
				return BadRequest();
			}

			var story = await service.GetByIdAsync(id);

			if (story == null)
			{
				return NotFound();
			}

			return View(story);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(StoryEditDto model)
		{
			if (ModelState.IsValid == false)
			{
				return View(model);
			}

			bool success = await service.EditAsync(model);

			if (!success)
			{
				return NotFound();
			}

			return RedirectToAction(nameof(Details), new { Id = model.Id });
		}

		[HttpGet]
		public async Task<IActionResult> Delete(Guid id)
		{
			if (id == Guid.Empty)
			{
				return BadRequest();
			}

			var story = await service.GetByIdAsync(id);

			if (story == null)
			{
				return NotFound();
			}

			return View(story);
		}

		[HttpPost]
		public async Task<IActionResult> DeleteConfirmed(Guid id)
		{
			if (id == Guid.Empty)
			{
				return BadRequest();
			}

			bool deleted = await service.SoftDeleteAsync(id);

			if (!deleted)
			{
				return NotFound();
			}

			return RedirectToAction(nameof(Index));
		}
	}
}
