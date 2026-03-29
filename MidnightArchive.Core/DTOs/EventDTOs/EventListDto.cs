namespace MidnightArchive.Core.DTOs.EventDTOs
{
	public class EventListDto
	{
		public Guid Id { get; set; }

		public string Title { get; set; } = null!;

		public string? Location { get; set; }

		public string Creator { get; set; } = null!;

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public int ParticipantsCounts { get; set; }
	}
}