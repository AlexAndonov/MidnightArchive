using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Infra.Data.Models
{
	public class StoryLike
	{
		public Guid StoryId { get; set; }
		public Story Story { get; set; } = null!;

		public string UserId { get; set; } = null!;
		public ApplicationUser User { get; set; } = null!;
	}
}
