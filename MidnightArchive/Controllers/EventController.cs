using Microsoft.AspNetCore.Mvc;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.EventDTOs;
using MidnightArchive.Infra.Data.Enums;
using System.Security.Claims;

namespace MidnightArchive.Controllers
{
	public class EventController : Controller
	{
		private readonly IEventService service;

		public EventController(IEventService _service)
		{
			service = _service;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			IEnumerable<EventListDto> events = await service.GetAllAsync();
			return View(events);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(EventCreateDto model)
		{
			if (ModelState.IsValid == false)
				return View(model);

			string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrWhiteSpace(userId))
				return Unauthorized();

			try
			{
				EventDetailsDto eventModel = await service.AddAsync(model, userId);
				return RedirectToAction("Details", new { id = eventModel.Id });
			}
			catch (ArgumentException ex)
			{
				ModelState.AddModelError("", ex.Message);
				return View(model);
			}			
		}

		[HttpGet]
		public async Task<IActionResult> Details(Guid id)
		{
			EventDetailsDto? eventModel = await service.GetByIdAsync(id);

			if (eventModel == null)
				return NotFound();

			return View(eventModel);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(Guid id)
		{
			EventEditDto? eventModel = await service.GetByIdForEditAsync(id);

			if (eventModel == null)
				return NotFound();

			return View(eventModel);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(EventEditDto model)
		{
			if (ModelState.IsValid == false)
				return View(model);

			try
			{
				EventOperationResult result = await service.EditAsync(model);

				if (result == EventOperationResult.NotFound)
					return NotFound();
			}
			catch (ArgumentException ex)
			{
				ModelState.AddModelError("", ex.Message);
				return View(model);
			}

			return RedirectToAction(nameof(Details), new { id = model.Id });
		}

		[HttpGet]
		public async Task<IActionResult> SoftDelete(Guid id)
		{
			EventDetailsDto? eventModel = await service.GetByIdAsync(id);

			if (eventModel == null)
				return NotFound();

			return View(eventModel);
		}

		[HttpPost, ActionName("SoftDelete")]
		public async Task<IActionResult> SoftDeleteConfirmed(Guid id)
		{
			EventOperationResult result = await service.SoftDeleteAsync(id);

			if (result == EventOperationResult.NotFound)
				return NotFound();

			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public async Task<IActionResult> HardDelete(Guid id)
		{
			EventDetailsDto? eventModel= await service.GetByIdAsync(id);

			if (eventModel == null)
				return NotFound();

			return View(eventModel);
		}

		[HttpPost, ActionName("HardDelete")]
		public async Task<IActionResult> HardDeleteConfirmed(Guid id)
		{
			EventOperationResult result = await service.HardDeleteAsync(id);

			if (result == EventOperationResult.NotFound)
				return NotFound();

			return RedirectToAction(nameof(Index));
		}
	}
}
