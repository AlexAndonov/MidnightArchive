using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.ReportDTOs;
using MidnightArchive.Infra.Data.Enums;
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

			ReportOperationResult result = await reportService.CreateAsync(model, userId);

			switch (result)
			{
				case ReportOperationResult.Success:
					TempData["SuccessMessage"] = "Report submitted successfully.";
					return RedirectToAction("Details", "Story", new { id = model.StoryId });

				case ReportOperationResult.AlreadyReported:
					ModelState.AddModelError(string.Empty, "You have already reported this story.");
					break;

				case ReportOperationResult.StoryNotFound:
					return NotFound();

				default:
					ModelState.AddModelError(string.Empty, "Something went wrong while submitting the report.");
					break;
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
			ReportOperationResult result = await reportService.ResolveAsync(id);

			switch (result)
			{
				case ReportOperationResult.Success:
					TempData["SuccessMessage"] = "Report resolved successfully.";
					break;

				case ReportOperationResult.ReportNotFound:
					TempData["ErrorMessage"] = "Report not found.";
					break;

				default:
					TempData["ErrorMessage"] = "Something went wrong while resolving the report.";
					break;
			}

			return RedirectToAction(nameof(Index));
		}
	}
}