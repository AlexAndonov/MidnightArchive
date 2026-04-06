namespace MidnightArchive.Core.DTOs.EventDTOs
{
	public class EventDetailsDto
	{
		public Guid Id { get; set; }

		public string Title { get; set; } = null!;
		public string Description { get; set; } = null!;

		public string? Location { get; set; }

		public string CreatorId { get; set; } = null!;

		public string CreatorName { get; set; } = null!;

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public int ParticipantsCounts { get; set; }

		public bool IsJoinedByCurrentUser { get; set; }
	}
}
