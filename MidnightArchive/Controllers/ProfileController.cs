using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.HomeDTOs;
using System.Security.Claims;

namespace MidnightArchive.Controllers
{
	[Authorize]
	public class ProfileController : Controller
	{
		private readonly IProfileService profileService;

		public ProfileController(IProfileService _profileService)
		{
			profileService = _profileService;
		}

		[HttpGet]
		public async Task<IActionResult> MyProfile()
		{
			string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrWhiteSpace(userId))
			{
				return Unauthorized();
			}

			bool isAdmin = User.IsInRole("Admin");

			UserProfileDto? model = await profileService.GetMyProfileAsync(userId, isAdmin);

			if (model == null)
			{
				return NotFound();
			}

			return View(model);
		}
	}
}
