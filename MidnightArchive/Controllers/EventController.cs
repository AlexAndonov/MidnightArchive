using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.EventDTOs;
using MidnightArchive.Infra.Data.Enums;
using System.Security.Claims;

namespace MidnightArchive.Controllers
{
	[Authorize]
	public class EventController : Controller
	{
		private readonly IEventService service;

		public EventController(IEventService _service)
		{
			service = _service;
		}

		[HttpGet]
		[AllowAnonymous]
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
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(EventCreateDto model)
		{
			if (ModelState.IsValid == false)
				return View(model);

			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

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
		[AllowAnonymous]
		public async Task<IActionResult> Details(Guid id)
		{
			string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			EventDetailsDto? eventModel = await service.GetByIdAsync(id, userId);

			if (eventModel == null)
			{
				return NotFound();
			}

			return View(eventModel);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(Guid id)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

			bool isOwner = await service.IsOwnerAsync(id, userId);

			if (!isOwner)
				return Forbid();

			EventEditDto? eventModel = await service.GetByIdForEditAsync(id);

			if (eventModel == null)
				return NotFound();

			return View(eventModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(EventEditDto model)
		{
			if (ModelState.IsValid == false)
				return View(model);

			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

			EventOperationResult result = await service.EditAsync(model, userId);

			switch (result)
			{
				case EventOperationResult.Success:
					return RedirectToAction(nameof(Details), new { id = model.Id });

				case EventOperationResult.NotFound:
					return NotFound();

				case EventOperationResult.NotOwner:
					return Forbid();

				default:
					ModelState.AddModelError("", "Something went wrong.");
					return View(model);
			}
		}

		[HttpGet]
		public async Task<IActionResult> SoftDelete(Guid id)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

			EventDetailsDto? eventModel = await service.GetByIdAsync(id, userId);

			if (eventModel == null)
				return NotFound();

			if (eventModel.CreatorId != userId)
				return Forbid();

			return View(eventModel);
		}

		[HttpPost, ActionName("SoftDelete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SoftDeleteConfirmed(Guid id)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

			EventOperationResult result = await service.SoftDeleteAsync(id, userId);

			switch (result)
			{
				case EventOperationResult.Success:
					return RedirectToAction(nameof(Index));

				case EventOperationResult.NotFound:
					return NotFound();

				case EventOperationResult.NotOwner:
					return Forbid();

				default:
					return BadRequest();
			}
		}

		[HttpGet]
		public async Task<IActionResult> HardDelete(Guid id)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

			EventDetailsDto? eventModel = await service.GetByIdAsync(id, userId);

			if (eventModel == null)
				return NotFound();

			if (eventModel.CreatorId != userId)
				return Forbid();

			return View(eventModel);
		}

		[HttpPost, ActionName("HardDelete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> HardDeleteConfirmed(Guid id)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

			EventOperationResult result = await service.HardDeleteAsync(id, userId);

			switch (result)
			{
				case EventOperationResult.Success:
					return RedirectToAction(nameof(Index));

				case EventOperationResult.NotFound:
					return NotFound();

				case EventOperationResult.NotOwner:
					return Forbid();

				default:
					return BadRequest();
			}
		}

		[HttpPost]
		public async Task<IActionResult> Join(Guid id)
		{
			string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrWhiteSpace(userId))
			{
				return Unauthorized();
			}

			EventJoinResult result = await service.JoinAsync(id, userId);

			switch (result)
			{
				case EventJoinResult.NotFound:
					return NotFound();

				case EventJoinResult.OwnEvent:
					TempData["ErrorMessage"] = "You cannot join your own event.";
					break;

				case EventJoinResult.EventEnded:
					TempData["ErrorMessage"] = "You cannot join an event that has already ended.";
					break;

				case EventJoinResult.AlreadyJoined:
					TempData["ErrorMessage"] = "You have already joined this event.";
					break;

				case EventJoinResult.Success:
					TempData["SuccessMessage"] = "You joined the event successfully.";
					break;

				default:
					TempData["ErrorMessage"] = "Something went wrong.";
					break;
			}

			return RedirectToAction(nameof(Details), new { id });
		}

		[HttpPost]
		public async Task<IActionResult> Leave(Guid id)
		{
			string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrWhiteSpace(userId))
			{
				return Unauthorized();
			}

			EventLeaveResult result = await service.LeaveAsync(id, userId);

			switch (result)
			{
				case EventLeaveResult.NotFound:
					return NotFound();

				case EventLeaveResult.OwnEvent:
					TempData["ErrorMessage"] = "Event creator cannot leave their own event.";
					break;

				case EventLeaveResult.NotJoined:
					TempData["ErrorMessage"] = "You have not joined this event.";
					break;

				case EventLeaveResult.Success:
					TempData["SuccessMessage"] = "You left the event successfully.";
					break;

				default:
					TempData["ErrorMessage"] = "Something went wrong.";
					break;
			}

			return RedirectToAction(nameof(Details), new { id });
		}
	}
}
