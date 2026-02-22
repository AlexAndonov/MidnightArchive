using Microsoft.AspNetCore.Identity;

namespace MidnightArchive.Data.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string? Bio { get; set; }
		public string? ImageUrl { get; set; }
		public DateTime CreatedOn { get; set; }
		public bool IsDeleted { get; set; }
	}
}
