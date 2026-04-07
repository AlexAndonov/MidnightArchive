using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.DTOs.CommentDTOs;
using MidnightArchive.Core.Services;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Enums;
using MidnightArchive.Infra.Data.Models;
using MidnightArchive.Tests.Helpers;

namespace MidnightArchive.Tests.Services
{
    public class CommentServiceTests
    {
        private readonly IMapper mapper;

        public CommentServiceTests()
        {
            mapper = MapperFactory.Create();
        }

        private CommentService CreateService(ApplicationDbContext context)
            => new CommentService(context, mapper);

        [Fact]
        public async Task AddAsync_ShouldReturnNotFound_WhenStoryDoesNotExist()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            CommentService service = CreateService(context);

            var model = new CommentCreateDto
            {
                Content = "Valid comment content",
                StoryId = Guid.NewGuid()
            };

            CommentOperationResult result = await service.AddAsync(model, "user-1");

            result.Should().Be(CommentOperationResult.NotFound);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnNotFound_WhenStoryIsDeleted()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            CommentService service = CreateService(context);

            var story = new Story
            {
                Id = Guid.NewGuid(),
                Title = "Deleted story",
                Content = "Story content",
                AuthorId = "author-1",
                CategoryId = 1,
                IsDeleted = true
            };

            await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var model = new CommentCreateDto
            {
                Content = "Valid comment content",
                StoryId = story.Id
            };

            CommentOperationResult result = await service.AddAsync(model, "user-1");

            result.Should().Be(CommentOperationResult.NotFound);
        }

        [Fact]
        public async Task AddAsync_ShouldCreateComment_WhenStoryExists()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            CommentService service = CreateService(context);

            var story = new Story
            {
                Id = Guid.NewGuid(),
                Title = "Story",
                Content = "Story content",
                AuthorId = "author-1",
                CategoryId = 1,
                IsDeleted = false
            };

            await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var model = new CommentCreateDto
            {
                Content = "This is a valid comment.",
                StoryId = story.Id
            };

            CommentOperationResult result = await service.AddAsync(model, "user-1");

            Comment? commentInDb = await context.Comments
                .FirstOrDefaultAsync(c => c.StoryId == story.Id && c.AuthorId == "user-1", TestContext.Current.CancellationToken);

            result.Should().Be(CommentOperationResult.Success);
            commentInDb.Should().NotBeNull();
            commentInDb!.Content.Should().Be(model.Content);
            commentInDb.AuthorId.Should().Be("user-1");
            commentInDb.StoryId.Should().Be(story.Id);
            commentInDb.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task EditAsync_ShouldReturnNotFound_WhenCommentDoesNotExist()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            CommentService service = CreateService(context);

            var model = new CommentEditDto
            {
                Id = Guid.NewGuid(),
                Content = "Edited content",
                StoryId = Guid.NewGuid()
            };

            CommentOperationResult result = await service.EditAsync(model, "user-1");

            result.Should().Be(CommentOperationResult.NotFound);
        }

        [Fact]
        public async Task EditAsync_ShouldReturnNotTheAuthor_WhenUserIsNotAuthor()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            CommentService service = CreateService(context);

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = "Original content",
                CreatedOn = DateTime.UtcNow.AddDays(-1),
                AuthorId = "author-1",
                StoryId = Guid.NewGuid()
            };

            await context.Comments.AddAsync(comment, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var model = new CommentEditDto
            {
                Id = comment.Id,
                Content = "Edited content",
                StoryId = comment.StoryId
            };

            CommentOperationResult result = await service.EditAsync(model, "other-user");

            Comment? commentInDb = await context.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id, TestContext.Current.CancellationToken);

            result.Should().Be(CommentOperationResult.NotTheAuthor);
            commentInDb.Should().NotBeNull();
            commentInDb!.Content.Should().Be("Original content");
            commentInDb.ModifiedOn.Should().BeNull();
        }

        [Fact]
        public async Task EditAsync_ShouldUpdateComment_WhenUserIsAuthor()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            CommentService service = CreateService(context);

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = "Original content",
                CreatedOn = DateTime.UtcNow.AddDays(-1),
                AuthorId = "author-1",
                StoryId = Guid.NewGuid()
            };

            await context.Comments.AddAsync(comment, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var model = new CommentEditDto
            {
                Id = comment.Id,
                Content = "Updated content",
                StoryId = comment.StoryId
            };

            CommentOperationResult result = await service.EditAsync(model, "author-1");

            Comment? updatedComment = await context.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id, TestContext.Current.CancellationToken);

            result.Should().Be(CommentOperationResult.Success);
            updatedComment.Should().NotBeNull();
            updatedComment!.Content.Should().Be("Updated content");
            updatedComment.ModifiedOn.Should().NotBeNull();
        }

        [Fact]
        public async Task HardDeleteAsync_ShouldReturnNotFound_WhenCommentDoesNotExist()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            CommentService service = CreateService(context);

            CommentOperationResult result = await service.HardDeleteAsync(Guid.NewGuid(), "user-1");

            result.Should().Be(CommentOperationResult.NotFound);
        }

        [Fact]
        public async Task HardDeleteAsync_ShouldReturnNotTheAuthor_WhenUserIsNotAuthor()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            CommentService service = CreateService(context);

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = "Comment content",
                CreatedOn = DateTime.UtcNow.AddDays(-1),
                AuthorId = "author-1",
                StoryId = Guid.NewGuid()
            };

            await context.Comments.AddAsync(comment, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            CommentOperationResult result = await service.HardDeleteAsync(comment.Id, "other-user");

            Comment? commentInDb = await context.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id, TestContext.Current.CancellationToken);

            result.Should().Be(CommentOperationResult.NotTheAuthor);
            commentInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task HardDeleteAsync_ShouldRemoveComment_WhenUserIsAuthor()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            CommentService service = CreateService(context);

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = "Comment content",
                CreatedOn = DateTime.UtcNow.AddDays(-1),
                AuthorId = "author-1",
                StoryId = Guid.NewGuid()
            };

            await context.Comments.AddAsync(comment, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            CommentOperationResult result = await service.HardDeleteAsync(comment.Id, "author-1");

            Comment? deletedComment = await context.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id, TestContext.Current.CancellationToken);

            result.Should().Be(CommentOperationResult.Success);
            deletedComment.Should().BeNull();
        }

        [Fact]
        public async Task GetAllForStoryAsync_ShouldReturnEmptyCollection_WhenStoryHasNoComments()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            CommentService service = CreateService(context);

            IEnumerable<CommentDto> result = await service.GetAllForStoryAsync(Guid.NewGuid());

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllForStoryAsync_ShouldReturnOnlyCommentsForGivenStory_OrderedByCreatedOnDescending()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            CommentService service = CreateService(context);

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "testuser",
                Email = "test@test.com"
            };

            Guid targetStoryId = Guid.NewGuid();
            Guid otherStoryId = Guid.NewGuid();

            var olderComment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = "Older comment",
                CreatedOn = DateTime.UtcNow.AddHours(-3),
                AuthorId = user.Id,
                Author = user,
                StoryId = targetStoryId
            };

            var newerComment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = "Newer comment",
                CreatedOn = DateTime.UtcNow.AddHours(-1),
                AuthorId = user.Id,
                Author = user,
                StoryId = targetStoryId
            };

            var otherStoryComment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = "Other story comment",
                CreatedOn = DateTime.UtcNow.AddHours(-2),
                AuthorId = user.Id,
                Author = user,
                StoryId = otherStoryId
            };

            await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
            await context.Comments.AddRangeAsync(olderComment, newerComment, otherStoryComment);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            List<CommentDto> result = (await service.GetAllForStoryAsync(targetStoryId)).ToList();

            result.Should().HaveCount(2);
            result[0].Content.Should().Be("Newer comment");
            result[1].Content.Should().Be("Older comment");
            result.Should().OnlyContain(c => c.StoryId == targetStoryId);
        }
    }
}