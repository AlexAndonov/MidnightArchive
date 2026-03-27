using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MidnightArchive.Infra.Data.Models
{
	[Comment("Join table for Events and Users")]
	public class EventParticipant
	{
		[Comment("Unique identifier of the event")]
		public Guid EventId { get; set; }

		[Comment("Navigation property for the event")]
		public Event Event { get; set; } = null!;

		[Comment("Unique identifier for the user")]
		[Required]
		public string ParticipantId { get; set; } = null!;

		[Comment("Navigation property for the user")]
		public ApplicationUser Participant { get; set; } = null!;


		[Comment("Indicates when the user joined the event")]
		public DateTime JoinedOn { get; set; }
	}
}
