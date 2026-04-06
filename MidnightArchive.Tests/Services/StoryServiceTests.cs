using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.DTOs.StoryDTOs;
using MidnightArchive.Core.Services;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Enums;
using MidnightArchive.Infra.Data.Models;
using MidnightArchive.Tests.Helpers;

namespace MidnightArchive.Tests.Services
{
	public class StoryServiceTests
	{
		private readonly IMapper mapper;
		private readonly FakeCacheService cacheService;

		public StoryServiceTests()
		{
			mapper = MapperFactory.Create();
			cacheService = new FakeCacheService();
		}

		private StoryService CreateService(ApplicationDbContext context)
			=> new StoryService(context, mapper, cacheService);

		[Fact]
		public async Task AddAsync_ShouldCreateStory_WhenInputIsValid()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			var category = new Category
			{
				Id = 1,
				Title = "Horror",
				Description = "Horror stories"
			};

			var user = new ApplicationUser
			{
				Id = "user-1",
				UserName = "testuser",
				Email = "test@test.com"
			};

			await context.Categories.AddAsync(category, TestContext.Current.CancellationToken);
			await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var model = new StoryCreateDto
			{
				Title = "My Story",
				Content = "This is a valid story content.",
				CategoryId = category.Id,
				IsAnonymous = false
			};

			StoryDetailDto result = await service.AddAsync(model, user.Id);

			Story? storyInDb = await context.Stories.FirstOrDefaultAsync(s => s.Id == result.Id, TestContext.Current.CancellationToken);

			storyInDb.Should().NotBeNull();
			storyInDb!.Title.Should().Be(model.Title);
			storyInDb.Content.Should().Be(model.Content);
			storyInDb.AuthorId.Should().Be(user.Id);
			storyInDb.CategoryId.Should().Be(category.Id);
			storyInDb.IsDeleted.Should().BeFalse();
		}

		[Fact]
		public async Task EditAsync_ShouldReturnNotFound_WhenStoryDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			var model = new StoryFormDto
			{
				Id = Guid.NewGuid(),
				Title = "Edited title",
				Content = "Edited content",
				CategoryId = 1
			};

			StoryOperationResult result = await service.EditAsync(model);

			result.Should().Be(StoryOperationResult.NotFound);
		}

		[Fact]
		public async Task EditAsync_ShouldUpdateStory_WhenStoryExists()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			var story = new Story
			{
				Id = Guid.NewGuid(),
				Title = "Old title",
				Content = "Old content",
				AuthorId = "user-1",
				CategoryId = 1,
				IsDeleted = false
			};

			await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var model = new StoryFormDto
			{
				Id = story.Id,
				Title = "New title",
				Content = "New content",
				CategoryId = 2
			};

			StoryOperationResult result = await service.EditAsync(model);

			Story? updatedStory = await context.Stories.FirstOrDefaultAsync(s => s.Id == story.Id, TestContext.Current.CancellationToken);

			result.Should().Be(StoryOperationResult.Success);
			updatedStory.Should().NotBeNull();
			updatedStory!.Title.Should().Be("New title");
			updatedStory.Content.Should().Be("New content");
			updatedStory.CategoryId.Should().Be(2);
			updatedStory.ModifiedOn.Should().NotBeNull();
		}

		[Fact]
		public async Task GetByIdAsync_ShouldReturnNull_WhenStoryDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			StoryDetailDto? result = await service.GetByIdAsync(Guid.NewGuid(), null);

			result.Should().BeNull();
		}

		[Fact]
		public async Task GetByIdForEditAsync_ShouldReturnNull_WhenStoryIsDeleted()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			var story = new Story
			{
				Id = Guid.NewGuid(),
				Title = "Deleted story",
				Content = "Content",
				AuthorId = "user-1",
				CategoryId = 1,
				IsDeleted = true
			};

			await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			StoryFormDto? result = await service.GetByIdForEditAsync(story.Id);

			result.Should().BeNull();
		}

		[Fact]
		public async Task GetRandomStoryIdAsync_ShouldReturnNull_WhenThereAreNoStories()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			Guid? result = await service.GetRandomStoryIdAsync();

			result.Should().BeNull();
		}

		[Fact]
		public async Task GetRandomStoryIdAsync_ShouldReturnExistingStoryId_WhenStoriesExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			var story1 = new Story
			{
				Id = Guid.NewGuid(),
				Title = "Story 1",
				Content = "Content 1",
				AuthorId = "user-1",
				CategoryId = 1,
				IsDeleted = false
			};

			var story2 = new Story
			{
				Id = Guid.NewGuid(),
				Title = "Story 2",
				Content = "Content 2",
				AuthorId = "user-2",
				CategoryId = 1,
				IsDeleted = false
			};

			await context.Stories.AddRangeAsync(story1, story2);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			Guid? result = await service.GetRandomStoryIdAsync();

			result.Should().NotBeNull();

			Guid actual = result.Value;

			(new[] { story1.Id, story2.Id }).Should().Contain(actual);
		}

		[Fact]
		public async Task SoftDeleteAsync_ShouldReturnNotFound_WhenStoryDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			StoryOperationResult result = await service.SoftDeleteAsync(Guid.NewGuid());

			result.Should().Be(StoryOperationResult.NotFound);
		}

		[Fact]
		public async Task SoftDeleteAsync_ShouldMarkStoryAsDeleted_WhenStoryExists()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			var story = new Story
			{
				Id = Guid.NewGuid(),
				Title = "Story",
				Content = "Content",
				AuthorId = "user-1",
				CategoryId = 1,
				IsDeleted = false
			};

			await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			StoryOperationResult result = await service.SoftDeleteAsync(story.Id);

			Story? deletedStory = await context.Stories.FirstOrDefaultAsync(s => s.Id == story.Id, TestContext.Current.CancellationToken);

			result.Should().Be(StoryOperationResult.Success);
			deletedStory.Should().NotBeNull();
			deletedStory!.IsDeleted.Should().BeTrue();
		}

		[Fact]
		public async Task HardDeleteAsync_ShouldReturnNotFound_WhenStoryDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			StoryOperationResult result = await service.HardDeleteAsync(Guid.NewGuid());

			result.Should().Be(StoryOperationResult.NotFound);
		}

		[Fact]
		public async Task HardDeleteAsync_ShouldRemoveStory_WhenStoryExists()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			var story = new Story
			{
				Id = Guid.NewGuid(),
				Title = "Story",
				Content = "Content",
				AuthorId = "user-1",
				CategoryId = 1,
				IsDeleted = false
			};

			await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			StoryOperationResult result = await service.HardDeleteAsync(story.Id);

			Story? deletedStory = await context.Stories.FirstOrDefaultAsync(s => s.Id == story.Id, TestContext.Current.CancellationToken);

			result.Should().Be(StoryOperationResult.Success);
			deletedStory.Should().BeNull();
		}

		[Fact]
		public async Task IncrementViewsAsync_ShouldReturnNotFound_WhenStoryDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			StoryOperationResult result = await service.IncrementViewsAsync(Guid.NewGuid());

			result.Should().Be(StoryOperationResult.NotFound);
		}

		[Fact]
		public async Task IncrementViewsAsync_ShouldIncreaseViews_WhenStoryExists()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			var story = new Story
			{
				Id = Guid.NewGuid(),
				Title = "Story",
				Content = "Content",
				AuthorId = "user-1",
				CategoryId = 1,
				ViewsCount = 0,
				IsDeleted = false
			};

			await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			StoryOperationResult result = await service.IncrementViewsAsync(story.Id);

			Story? updatedStory = await context.Stories.FirstOrDefaultAsync(s => s.Id == story.Id, TestContext.Current.CancellationToken);

			result.Should().Be(StoryOperationResult.Success);
			updatedStory!.ViewsCount.Should().Be(1);
		}

		[Fact]
		public async Task IsAuthorAsync_ShouldReturnTrue_WhenUserIsAuthor()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			var story = new Story
			{
				Id = Guid.NewGuid(),
				Title = "Story",
				Content = "Content",
				AuthorId = "author-1",
				CategoryId = 1,
				IsDeleted = false
			};

			await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			bool result = await service.IsAuthorAsync(story.Id, "author-1");

			result.Should().BeTrue();
		}

		[Fact]
		public async Task IsAuthorAsync_ShouldReturnFalse_WhenUserIsNotAuthor()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			var story = new Story
			{
				Id = Guid.NewGuid(),
				Title = "Story",
				Content = "Content",
				AuthorId = "author-1",
				CategoryId = 1,
				IsDeleted = false
			};

			await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			bool result = await service.IsAuthorAsync(story.Id, "other-user");

			result.Should().BeFalse();
		}

		[Fact]
		public async Task LikeAsync_ShouldReturnNotFound_WhenStoryDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			StoryOperationResult result = await service.LikeAsync(Guid.NewGuid(), "user-1");

			result.Should().Be(StoryOperationResult.NotFound);
		}

		[Fact]
		public async Task LikeAsync_ShouldReturnAlreadyLiked_WhenUserAlreadyLikedStory()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			var story = new Story
			{
				Id = Guid.NewGuid(),
				Title = "Story",
				Content = "Content",
				AuthorId = "author-1",
				CategoryId = 1,
				LikesCount = 1,
				IsDeleted = false
			};

			var like = new StoryLike
			{
				StoryId = story.Id,
				UserId = "user-1"
			};

			await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
			await context.StoryLikes.AddAsync(like, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			StoryOperationResult result = await service.LikeAsync(story.Id, "user-1");

			result.Should().Be(StoryOperationResult.AlreadyLiked);
		}

		[Fact]
		public async Task LikeAsync_ShouldCreateLikeAndIncreaseLikesCount_WhenValid()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			var story = new Story
			{
				Id = Guid.NewGuid(),
				Title = "Story",
				Content = "Content",
				AuthorId = "author-1",
				CategoryId = 1,
				LikesCount = 0,
				IsDeleted = false
			};

			await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			StoryOperationResult result = await service.LikeAsync(story.Id, "user-1");

			StoryLike? like = await context.StoryLikes
				.FirstOrDefaultAsync(sl => sl.StoryId == story.Id && sl.UserId == "user-1", TestContext.Current.CancellationToken);

			Story? updatedStory = await context.Stories.FirstOrDefaultAsync(s => s.Id == story.Id, TestContext.Current.CancellationToken);

			result.Should().Be(StoryOperationResult.Success);
			like.Should().NotBeNull();
			updatedStory!.LikesCount.Should().Be(1);
		}

		[Fact]
		public async Task UnlikeAsync_ShouldReturnLikeNotFound_WhenLikeDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			StoryOperationResult result = await service.UnlikeAsync(Guid.NewGuid(), "user-1");

			result.Should().Be(StoryOperationResult.LikeNotFound);
		}

		[Fact]
		public async Task UnlikeAsync_ShouldReturnNotFound_WhenStoryDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			Guid storyId = Guid.NewGuid();

			await context.StoryLikes.AddAsync(new StoryLike
			{
				StoryId = storyId,
				UserId = "user-1",
			}, TestContext.Current.CancellationToken);

			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			StoryOperationResult result = await service.UnlikeAsync(storyId, "user-1");

			result.Should().Be(StoryOperationResult.NotFound);
		}

		[Fact]
		public async Task UnlikeAsync_ShouldRemoveLikeAndDecreaseLikesCount_WhenValid()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			var story = new Story
			{
				Id = Guid.NewGuid(),
				Title = "Story",
				Content = "Content",
				AuthorId = "author-1",
				CategoryId = 1,
				LikesCount = 1,
				IsDeleted = false
			};

			var like = new StoryLike
			{
				StoryId = story.Id,
				UserId = "user-1"
			};

			await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
			await context.StoryLikes.AddAsync(like, TestContext.Current.CancellationToken);
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			StoryOperationResult result = await service.UnlikeAsync(story.Id, "user-1");

			StoryLike? likeInDb = await context.StoryLikes
				.FirstOrDefaultAsync(sl => sl.StoryId == story.Id && sl.UserId == "user-1", TestContext.Current.CancellationToken);

			Story? updatedStory = await context.Stories.FirstOrDefaultAsync(s => s.Id == story.Id, TestContext.Current.CancellationToken);

			result.Should().Be(StoryOperationResult.Success);
			likeInDb.Should().BeNull();
			updatedStory!.LikesCount.Should().Be(0);
		}

		[Fact]
		public async Task HasUserLikedAsync_ShouldReturnTrue_WhenLikeExists()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			Guid storyId = Guid.NewGuid();

			await context.StoryLikes.AddAsync(new StoryLike
			{
				StoryId = storyId,
				UserId = "user-1"
			}, TestContext.Current.CancellationToken);

			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			bool result = await service.HasUserLikedAsync(storyId, "user-1");

			result.Should().BeTrue();
		}

		[Fact]
		public async Task HasUserLikedAsync_ShouldReturnFalse_WhenLikeDoesNotExist()
		{
			using ApplicationDbContext context = TestDbContextFactory.Create();
			StoryService service = CreateService(context);

			bool result = await service.HasUserLikedAsync(Guid.NewGuid(), "user-1");

			result.Should().BeFalse();
		}
	}
}