using Microsoft.AspNetCore.Mvc;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.CategoryDTOs;

namespace MidnightArchive.Controllers
{
	//[Authorize(Roles = "Admin")]
	public class CategoryController : Controller
	{
		private readonly ICategoryService service;

		public CategoryController(ICategoryService _service)
		{
			service = _service;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			IEnumerable<CategoryListDto> categories = await service.GetAllAsync();

			return View(categories);
		}

		[HttpGet]
		public async Task<IActionResult> Details(int Id)
		{
			if (Id <= 0)
			{
				return BadRequest();
			}

			CategoryDetailDto? category = await service.GetByIdAsync(Id);

			if (category == null)
			{
				return NotFound();
			}

			return View(category);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CategoryCreateDto model)
		{
			if (ModelState.IsValid == false)
			{
				return View(model);
			}

			CategoryDto createdCategory = await service.AddAsync(model);

			return RedirectToAction(nameof(Details), new {Id = createdCategory.Id});
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int Id)
		{
			if (Id <= 0)
			{
				return BadRequest();
			}

			CategoryEditDto? category = await service.GetForEditAsync(Id);

			if (category == null)
			{
				return NotFound();
			}

			return View(category);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(CategoryEditDto model)
		{
			if (!ModelState.IsValid)
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
		public async Task<IActionResult> Delete(int Id)
		{
			if (Id <= 0)
			{
				return BadRequest();
			}

			CategoryDetailDto? category = await service.GetByIdAsync(Id);

			if (category == null)
			{
				return NotFound();
			}

			return View(category);
		}

		[HttpPost]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (id <= 0)
			{
				return BadRequest();
			}

			var deleted = await service.SoftDeleteAsync(id);

			if (!deleted)
			{
				return NotFound();
			}

			return RedirectToAction(nameof(Index));
		}
	}
}
