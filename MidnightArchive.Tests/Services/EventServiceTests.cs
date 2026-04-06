using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.DTOs.EventDTOs;
using MidnightArchive.Core.Services;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Enums;
using MidnightArchive.Infra.Data.Models;
using MidnightArchive.Tests.Helpers;
using Xunit;

namespace MidnightArchive.Tests.Services
{
	public class EventServiceTests
	{
		private readonly IMapper mapper;

		public EventServiceTests()
		{
			mapper = MapperFactory.Create();
		}

		private EventService CreateService(ApplicationDbContext context)
			=> new EventService(context, mapper);

		[Fact]
		public async Task AddAsync_ShouldCreateEvent_WhenInputIsValid()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var user = new ApplicationUser
			{
				Id = "creator-1",
				UserName = "creator",
				Email = "creator@test.com"
			};

			await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var model = new EventCreateDto
			{
				Title = "Test Event",
				Description = "Test Description",
				Location = "Sofia",
				StartDate = new DateTime(2026, 4, 10),
				EndDate = new DateTime(2026, 4, 12)
			};

			EventDetailsDto result = await service.AddAsync(model, user.Id);

			Event? eventInDb = await context.Events.FirstOrDefaultAsync(e => e.Id == result.Id, TestContext.Current.CancellationToken);

			eventInDb.Should().NotBeNull();
			eventInDb!.Title.Should().Be(model.Title);
			eventInDb.Description.Should().Be(model.Description);
			eventInDb.Location.Should().Be("Sofia");
			eventInDb.CreatorId.Should().Be(user.Id);
			eventInDb.StartDate.Should().Be(model.StartDate.Date);
			eventInDb.EndDate.Should().Be(model.EndDate.AddDays(1).AddTicks(-1));
		}

		[Fact]
		public async Task AddAsync_ShouldSetLocationToOnline_WhenLocationIsNullOrWhitespace()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var user = new ApplicationUser
			{
				Id = "creator-1",
				UserName = "creator",
				Email = "creator@test.com"
			};

			await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var model = new EventCreateDto
			{
				Title = "Test Event",
				Description = "Test Description",
				Location = "   ",
				StartDate = new DateTime(2026, 4, 10),
				EndDate = new DateTime(2026, 4, 12)
			};

			EventDetailsDto result = await service.AddAsync(model, user.Id);

			Event? eventInDb = await context.Events.FirstOrDefaultAsync(e => e.Id == result.Id, TestContext.Current.CancellationToken);

			eventInDb.Should().NotBeNull();
			eventInDb!.Location.Should().Be("Online");
		}

		[Fact]
		public async Task AddAsync_ShouldThrowArgumentException_WhenEndDateIsBeforeOrEqualToStartDate()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var model = new EventCreateDto
			{
				Title = "Test Event",
				Description = "Test Description",
				StartDate = new DateTime(2026, 4, 10),
				EndDate = new DateTime(2026, 4, 10)
			};

			Func<Task> act = async () => await service.AddAsync(model, "creator-1");

			await act.Should().ThrowAsync<ArgumentException>()
				.WithMessage("End date must be after Start date!");
		}

		[Fact]
		public async Task GetByIdAsync_ShouldReturnNull_WhenEventDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			EventDetailsDto? result = await service.GetByIdAsync(Guid.NewGuid(), null);

			result.Should().BeNull();
		}

		[Fact]
		public async Task GetByIdAsync_ShouldReturnEvent_WhenEventExists()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var user = new ApplicationUser
			{
				Id = "creator-1",
				UserName = "creator",
				Email = "creator@test.com"
			};

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event Title",
				Description = "Event Description",
				Location = "Online",
				CreatorId = user.Id,
				Creator = user,
				StartDate = new DateTime(2026, 4, 10),
				EndDate = new DateTime(2026, 4, 11),
				IsDeleted = false
			};

			await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventDetailsDto? result = await service.GetByIdAsync(eventEntity.Id, null);

			result.Should().NotBeNull();
			result!.Id.Should().Be(eventEntity.Id);
			result.Title.Should().Be(eventEntity.Title);
			result.CreatorId.Should().Be(user.Id);
			result.CreatorName.Should().Be(user.UserName);
			result.IsJoinedByCurrentUser.Should().BeFalse();
		}

		[Fact]
		public async Task GetByIdAsync_ShouldSetIsJoinedByCurrentUserToTrue_WhenUserHasJoined()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var creator = new ApplicationUser
			{
				Id = "creator-1",
				UserName = "creator",
				Email = "creator@test.com"
			};

			var participant = new ApplicationUser
			{
				Id = "user-1",
				UserName = "participant",
				Email = "participant@test.com"
			};

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event Title",
				Description = "Event Description",
				Location = "Online",
				CreatorId = creator.Id,
				Creator = creator,
				StartDate = DateTime.UtcNow.AddDays(2),
				EndDate = DateTime.UtcNow.AddDays(3),
				IsDeleted = false
			};

			var join = new EventParticipant
			{
				EventId = eventEntity.Id,
				ParticipantId = participant.Id,
				JoinedOn = DateTime.UtcNow
			};

			await context.Users.AddRangeAsync(creator, participant);
			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.EventParticipants.AddAsync(join, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventDetailsDto? result = await service.GetByIdAsync(eventEntity.Id, participant.Id);

			result.Should().NotBeNull();
			result!.IsJoinedByCurrentUser.Should().BeTrue();
		}

		[Fact]
		public async Task GetByIdForEditAsync_ShouldReturnNull_WhenEventIsDeleted()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Deleted Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2),
				IsDeleted = true
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventEditDto? result = await service.GetByIdForEditAsync(eventEntity.Id);

			result.Should().BeNull();
		}

		[Fact]
		public async Task GetByIdForEditAsync_ShouldReturnEvent_WhenEventExists()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Editable Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventEditDto? result = await service.GetByIdForEditAsync(eventEntity.Id);

			result.Should().NotBeNull();
			result!.Id.Should().Be(eventEntity.Id);
			result.Title.Should().Be(eventEntity.Title);
		}

		[Fact]
		public async Task EditAsync_ShouldReturnNotFound_WhenEventDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var model = new EventEditDto
			{
				Id = Guid.NewGuid(),
				Title = "Edited",
				Description = "Edited Desc",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2)
			};

			EventOperationResult result = await service.EditAsync(model, "creator-1");

			result.Should().Be(EventOperationResult.NotFound);
		}

		[Fact]
		public async Task EditAsync_ShouldReturnNotOwner_WhenUserIsNotOwner()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var model = new EventEditDto
			{
				Id = eventEntity.Id,
				Title = "Edited",
				Description = "Edited Desc",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2)
			};

			EventOperationResult result = await service.EditAsync(model, "other-user");

			result.Should().Be(EventOperationResult.NotOwner);
		}

		[Fact]
		public async Task EditAsync_ShouldReturnInvalidDateRange_WhenEndDateIsBeforeOrEqualToStartDate()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var model = new EventEditDto
			{
				Id = eventEntity.Id,
				Title = "Edited",
				Description = "Edited Desc",
				StartDate = new DateTime(2026, 4, 10),
				EndDate = new DateTime(2026, 4, 10)
			};

			EventOperationResult result = await service.EditAsync(model, "creator-1");

			result.Should().Be(EventOperationResult.InvalidDateRange);
		}

		[Fact]
		public async Task EditAsync_ShouldUpdateEvent_WhenInputIsValidAndUserIsOwner()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Old Title",
				Description = "Old Description",
				Location = "Old Location",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var model = new EventEditDto
			{
				Id = eventEntity.Id,
				Title = "New Title",
				Description = "New Description",
				Location = "New Location",
				StartDate = new DateTime(2026, 4, 20),
				EndDate = new DateTime(2026, 4, 21)
			};

			EventOperationResult result = await service.EditAsync(model, "creator-1");

			Event? updatedEvent = await context.Events.FirstOrDefaultAsync(e => e.Id == eventEntity.Id, TestContext.Current.CancellationToken);

			result.Should().Be(EventOperationResult.Success);
			updatedEvent.Should().NotBeNull();
			updatedEvent!.Title.Should().Be("New Title");
			updatedEvent.Description.Should().Be("New Description");
			updatedEvent.Location.Should().Be("New Location");
			updatedEvent.StartDate.Should().Be(model.StartDate);
			updatedEvent.EndDate.Should().Be(model.EndDate.Date.AddDays(1).AddTicks(-1));
		}

		[Fact]
		public async Task SoftDeleteAsync_ShouldReturnNotFound_WhenEventDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			EventOperationResult result = await service.SoftDeleteAsync(Guid.NewGuid(), "creator-1");

			result.Should().Be(EventOperationResult.NotFound);
		}

		[Fact]
		public async Task SoftDeleteAsync_ShouldReturnNotOwner_WhenUserIsNotOwner()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventOperationResult result = await service.SoftDeleteAsync(eventEntity.Id, "other-user");

			result.Should().Be(EventOperationResult.NotOwner);
		}

		[Fact]
		public async Task SoftDeleteAsync_ShouldMarkEventAsDeleted_WhenUserIsOwner()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventOperationResult result = await service.SoftDeleteAsync(eventEntity.Id, "creator-1");

			Event? deletedEvent = await context.Events.FirstOrDefaultAsync(e => e.Id == eventEntity.Id, TestContext.Current.CancellationToken);

			result.Should().Be(EventOperationResult.Success);
			deletedEvent.Should().NotBeNull();
			deletedEvent!.IsDeleted.Should().BeTrue();
		}

		[Fact]
		public async Task HardDeleteAsync_ShouldReturnNotFound_WhenEventDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			EventOperationResult result = await service.HardDeleteAsync(Guid.NewGuid(), "creator-1");

			result.Should().Be(EventOperationResult.NotFound);
		}

		[Fact]
		public async Task HardDeleteAsync_ShouldReturnNotOwner_WhenUserIsNotOwner()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventOperationResult result = await service.HardDeleteAsync(eventEntity.Id, "other-user");

			result.Should().Be(EventOperationResult.NotOwner);
		}

		[Fact]
		public async Task HardDeleteAsync_ShouldRemoveEvent_WhenUserIsOwner()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventOperationResult result = await service.HardDeleteAsync(eventEntity.Id, "creator-1");

			Event? deletedEvent = await context.Events.FirstOrDefaultAsync(e => e.Id == eventEntity.Id, TestContext.Current.CancellationToken);

			result.Should().Be(EventOperationResult.Success);
			deletedEvent.Should().BeNull();
		}

		[Fact]
		public async Task IsOwnerAsync_ShouldReturnTrue_WhenUserOwnsEvent()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			bool result = await service.IsOwnerAsync(eventEntity.Id, "creator-1");

			result.Should().BeTrue();
		}

		[Fact]
		public async Task IsOwnerAsync_ShouldReturnFalse_WhenUserDoesNotOwnEvent()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			bool result = await service.IsOwnerAsync(eventEntity.Id, "other-user");

			result.Should().BeFalse();
		}

		[Fact]
		public async Task JoinAsync_ShouldReturnNotFound_WhenEventDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			EventJoinResult result = await service.JoinAsync(Guid.NewGuid(), "user-1");

			result.Should().Be(EventJoinResult.NotFound);
		}

		[Fact]
		public async Task JoinAsync_ShouldReturnOwnEvent_WhenUserIsCreator()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(1),
				EndDate = DateTime.UtcNow.AddDays(2),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventJoinResult result = await service.JoinAsync(eventEntity.Id, "creator-1");

			result.Should().Be(EventJoinResult.OwnEvent);
		}

		[Fact]
		public async Task JoinAsync_ShouldReturnEventEnded_WhenEventHasEnded()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Past Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(-3),
				EndDate = DateTime.UtcNow.AddDays(-1),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventJoinResult result = await service.JoinAsync(eventEntity.Id, "user-1");

			result.Should().Be(EventJoinResult.EventEnded);
		}

		[Fact]
		public async Task JoinAsync_ShouldReturnAlreadyJoined_WhenUserAlreadyJoined()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(2),
				EndDate = DateTime.UtcNow.AddDays(3),
				IsDeleted = false
			};

			var participant = new EventParticipant
			{
				EventId = eventEntity.Id,
				ParticipantId = "user-1",
				JoinedOn = DateTime.UtcNow
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.EventParticipants.AddAsync(participant, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventJoinResult result = await service.JoinAsync(eventEntity.Id, "user-1");

			result.Should().Be(EventJoinResult.AlreadyJoined);
		}

		[Fact]
		public async Task JoinAsync_ShouldAddParticipant_WhenInputIsValid()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Future Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(2),
				EndDate = DateTime.UtcNow.AddDays(3),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventJoinResult result = await service.JoinAsync(eventEntity.Id, "user-1");

			EventParticipant? participant = await context.EventParticipants
				.FirstOrDefaultAsync(ep => ep.EventId == eventEntity.Id && ep.ParticipantId == "user-1", TestContext.Current.CancellationToken);

			result.Should().Be(EventJoinResult.Success);
			participant.Should().NotBeNull();
		}

		[Fact]
		public async Task LeaveAsync_ShouldReturnNotFound_WhenEventDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			EventLeaveResult result = await service.LeaveAsync(Guid.NewGuid(), "user-1");

			result.Should().Be(EventLeaveResult.NotFound);
		}

		[Fact]
		public async Task LeaveAsync_ShouldReturnOwnEvent_WhenUserIsCreator()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(2),
				EndDate = DateTime.UtcNow.AddDays(3),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventLeaveResult result = await service.LeaveAsync(eventEntity.Id, "creator-1");

			result.Should().Be(EventLeaveResult.OwnEvent);
		}

		[Fact]
		public async Task LeaveAsync_ShouldReturnNotJoined_WhenUserHasNotJoined()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(2),
				EndDate = DateTime.UtcNow.AddDays(3),
				IsDeleted = false
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventLeaveResult result = await service.LeaveAsync(eventEntity.Id, "user-1");

			result.Should().Be(EventLeaveResult.NotJoined);
		}

		[Fact]
		public async Task LeaveAsync_ShouldRemoveParticipant_WhenUserHasJoined()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			EventService service = CreateService(context);

			var eventEntity = new Event
			{
				Id = Guid.NewGuid(),
				Title = "Event",
				Description = "Description",
				CreatorId = "creator-1",
				StartDate = DateTime.UtcNow.AddDays(2),
				EndDate = DateTime.UtcNow.AddDays(3),
				IsDeleted = false
			};

			var participant = new EventParticipant
			{
				EventId = eventEntity.Id,
				ParticipantId = "user-1",
				JoinedOn = DateTime.UtcNow
			};

			await context.Events.AddAsync(eventEntity, TestContext.Current.CancellationToken);
			await context.EventParticipants.AddAsync(participant, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			EventLeaveResult result = await service.LeaveAsync(eventEntity.Id, "user-1");

			EventParticipant? participantInDb = await context.EventParticipants
				.FirstOrDefaultAsync(ep => ep.EventId == eventEntity.Id && ep.ParticipantId == "user-1", TestContext.Current.CancellationToken);

			result.Should().Be(EventLeaveResult.Success);
			participantInDb.Should().BeNull();
		}
	}
}