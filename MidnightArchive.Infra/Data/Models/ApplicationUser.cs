using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Infra.Data.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string? Bio { get; set; }
		public string? ImageUrl { get; set; }
		public DateTime CreatedOn { get; set; }
		public bool IsDeleted { get; set; }
		public ICollection<Story> Stories { get; set; } = new List<Story>();
		public ICollection<EventParticipant> EventParticipants { get; set; } = new List<EventParticipant>();
		public ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
	}
}
