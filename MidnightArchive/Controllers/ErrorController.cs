using Microsoft.AspNetCore.Mvc;
using MidnightArchive.Models;
using System.Diagnostics;

namespace MidnightArchive.Controllers
{
	public class ErrorController : Controller
	{
		[Route("Error/404")]
		public IActionResult NotFoundPage()
		{
			Response.StatusCode = 404;
			return View("NotFound");
		}

		[Route("Error/500")]
		public IActionResult InternalServerError()
		{
			Response.StatusCode = 500;
			return View("InternalServerError");
		}

		[Route("Error")]
		public IActionResult HandleError(int? statusCode = null)
		{
			if (statusCode == 404 || statusCode == 400)
			{
				Response.StatusCode = statusCode ?? 404;
				return View("NotFound");
			}

			Response.StatusCode = 500;
			return View("InternalServerError");
		}
	}
}
