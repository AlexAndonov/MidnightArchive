using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.ReportDTOs;
using System.Security.Claims;

namespace MidnightArchive.Controllers
{
	public class ReportController : Controller
	{
		private readonly IReportService reportService;

		public ReportController(IReportService _reportService)
		{
			reportService = _reportService;
		}

		[HttpGet]
		[Authorize]
		public IActionResult Create(Guid storyId)
		{
			ReportCreateDto model = new ReportCreateDto
			{
				StoryId = storyId
			};

			return View(model);
		}

		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ReportCreateDto model)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				await reportService.CreateAsync(model, userId);
				TempData["SuccessMessage"] = "Report submitted successfully.";
				return RedirectToAction("Details", "Story", new { id = model.StoryId });
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
			}
			catch (ArgumentException ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
			}

			return View(model);
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Index()
		{
			IEnumerable<ReportListDto> model = await reportService.GetAllAsync();
			return View(model);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Resolve(Guid id)
		{
			try
			{
				await reportService.ResolveAsync(id);
				TempData["SuccessMessage"] = "Report resolved successfully.";
			}
			catch (ArgumentException ex)
			{
				TempData["ErrorMessage"] = ex.Message;
			}

			return RedirectToAction(nameof(Index));
		}
	}
}