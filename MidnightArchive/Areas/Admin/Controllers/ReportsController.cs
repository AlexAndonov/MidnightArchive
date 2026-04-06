using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.ReportDTOs;

namespace MidnightArchive.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class ReportsController : Controller
	{
		private readonly IAdminService adminService;

		public ReportsController(IAdminService _adminService)
		{
			adminService = _adminService;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			IEnumerable<ReportedStoryAdminDto> reports = await adminService.GetReportedStoriesAsync();

			return View(reports);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Resolve(Guid storyId)
		{
			await adminService.ResolveReportsAsync(storyId);

			return RedirectToAction("Index", "Reports", new { area = "Admin" });
		}
	}
}