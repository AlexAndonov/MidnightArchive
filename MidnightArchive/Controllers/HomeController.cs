using Microsoft.AspNetCore.Mvc;
using MidnightArchive.Core.Contracts;
using MidnightArchive.Core.DTOs.Home;
using MidnightArchive.Models;
using System.Diagnostics;

namespace MidnightArchive.Controllers
{
	public class HomeController : Controller
	{
		private readonly IHomeService service;

		public HomeController(IHomeService _service)
		{
			service = _service;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			HomeIndexDto model = await service.GetHomePageDataAsync();
			return View(model);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
