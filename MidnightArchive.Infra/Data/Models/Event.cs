using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MidnightArchive.Infra.Data.Models
{
	[Comment("Event class")]
	public class Event
	{
		[Comment("Event unique identifier")]
		public Guid Id { get; set; }


		[Comment("Event title")]
		[Required]
		[MaxLength(100)]
		public string Title { get; set; } = null!;


		[Comment("Event description")]
		[Required]
		[MaxLength(1000)]
		public string Description { get; set; } = null!;


		[Comment("Location of the event. It is not mandatory, because event can be online!")]
		public string? Location { get; set; } 


		[Comment("Unique identifier of the event's creator")]
		[Required]
		public string CreatorId { get; set; } = null!;


		[Comment("Navigation property for the event creator")]
		public ApplicationUser Creator { get; set; } = null!;


		[Comment("Event start date")]
		[Required]
		public DateTime StartDate { get; set; }


		[Comment("Event end date")]
		[Required]
		public DateTime EndDate { get; set; }


		[Comment("Participants in the event")]
		public ICollection<EventParticipant> Participants { get; set; } = new List<EventParticipant>();
	}
}
