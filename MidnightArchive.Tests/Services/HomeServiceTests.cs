using AutoMapper;
using FluentAssertions;
using MidnightArchive.Core.DTOs.Home;
using MidnightArchive.Core.Services;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Models;
using MidnightArchive.Tests.Helpers;

namespace MidnightArchive.Tests.Services
{
    public class HomeServiceTests
    {
        private readonly IMapper mapper;

        public HomeServiceTests()
        {
            mapper = MapperFactory.Create();
        }

        private HomeService CreateService(ApplicationDbContext context)
            => new HomeService(context, mapper);

        [Fact]
        public async Task GetHomePageDataAsync_ShouldReturnEmptyCollections_WhenDatabaseIsEmpty()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            HomeService service = CreateService(context);

            HomeIndexDto result = await service.GetHomePageDataAsync();

            result.Should().NotBeNull();
            result.TopStoriesOfMonth.Should().BeEmpty();
            result.LatestStories.Should().BeEmpty();
            result.FeaturedCategories.Should().BeEmpty();
            result.FeaturedEvents.Should().BeEmpty();
        }

        [Fact]
        public async Task GetHomePageDataAsync_ShouldReturnOnlyNonDeletedLatestStories_OrderedByCreatedOnDescending()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            HomeService service = CreateService(context);

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "testuser",
                Email = "test@test.com"
            };

            var category = new Category
            {
                Id = 1,
                Title = "Horror",
                Description = "Horror stories",
                IsDeleted = false
            };

            var oldStory = new Story
            {
                Id = Guid.NewGuid(),
                Title = "Old Story",
                Content = "Old story content",
                AuthorId = user.Id,
                Author = user,
                CategoryId = category.Id,
                Category = category,
                CreatedOn = DateTime.UtcNow.AddDays(-5),
                IsDeleted = false
            };

            var newStory = new Story
            {
                Id = Guid.NewGuid(),
                Title = "New Story",
                Content = "New story content",
                AuthorId = user.Id,
                Author = user,
                CategoryId = category.Id,
                Category = category,
                CreatedOn = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false
            };

            var deletedStory = new Story
            {
                Id = Guid.NewGuid(),
                Title = "Deleted Story",
                Content = "Deleted story content",
                AuthorId = user.Id,
                Author = user,
                CategoryId = category.Id,
                Category = category,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = true
            };

            await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
            await context.Categories.AddAsync(category, TestContext.Current.CancellationToken);
            await context.Stories.AddRangeAsync(oldStory, newStory, deletedStory);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            HomeIndexDto result = await service.GetHomePageDataAsync();

            result.LatestStories.Should().HaveCount(2);
            result.LatestStories.Select(s => s.Title).Should().ContainInOrder("New Story", "Old Story");
            result.LatestStories.Select(s => s.Title).Should().NotContain("Deleted Story");
        }

        [Fact]
        public async Task GetHomePageDataAsync_ShouldReturnTopStoriesOfMonth_OnlyFromLast30Days_OrderedByViewsThenLikes()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            HomeService service = CreateService(context);

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "testuser",
                Email = "test@test.com"
            };

            var category = new Category
            {
                Id = 1,
                Title = "Thriller",
                Description = "Thriller stories",
                IsDeleted = false
            };

            var highViewsLowLikes = new Story
            {
                Id = Guid.NewGuid(),
                Title = "High Views Low Likes",
                Content = "Story content",
                AuthorId = user.Id,
                Author = user,
                CategoryId = category.Id,
                Category = category,
                CreatedOn = DateTime.UtcNow.AddDays(-10),
                ViewsCount = 100,
                LikesCount = 5,
                IsDeleted = false
            };

            var highViewsHighLikes = new Story
            {
                Id = Guid.NewGuid(),
                Title = "High Views High Likes",
                Content = "Story content",
                AuthorId = user.Id,
                Author = user,
                CategoryId = category.Id,
                Category = category,
                CreatedOn = DateTime.UtcNow.AddDays(-8),
                ViewsCount = 100,
                LikesCount = 20,
                IsDeleted = false
            };

            var lowerViews = new Story
            {
                Id = Guid.NewGuid(),
                Title = "Lower Views",
                Content = "Story content",
                AuthorId = user.Id,
                Author = user,
                CategoryId = category.Id,
                Category = category,
                CreatedOn = DateTime.UtcNow.AddDays(-6),
                ViewsCount = 50,
                LikesCount = 50,
                IsDeleted = false
            };

            var tooOldStory = new Story
            {
                Id = Guid.NewGuid(),
                Title = "Too Old Story",
                Content = "Story content",
                AuthorId = user.Id,
                Author = user,
                CategoryId = category.Id,
                Category = category,
                CreatedOn = DateTime.UtcNow.AddDays(-40),
                ViewsCount = 999,
                LikesCount = 999,
                IsDeleted = false
            };

            await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
            await context.Categories.AddAsync(category, TestContext.Current.CancellationToken);
            await context.Stories.AddRangeAsync(highViewsLowLikes, highViewsHighLikes, lowerViews, tooOldStory);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            HomeIndexDto result = await service.GetHomePageDataAsync();

            result.TopStoriesOfMonth.Should().HaveCount(3);
            result.TopStoriesOfMonth.Select(s => s.Title).Should()
                .ContainInOrder("High Views High Likes", "High Views Low Likes", "Lower Views");
            result.TopStoriesOfMonth.Select(s => s.Title).Should().NotContain("Too Old Story");
        }

        [Fact]
        public async Task GetHomePageDataAsync_ShouldReturnOnlyNonDeletedCategories_OrderedByActiveStoriesCount()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            HomeService service = CreateService(context);

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "testuser",
                Email = "test@test.com"
            };

            var horror = new Category
            {
                Id = 1,
                Title = "Horror",
                Description = "Horror stories",
                IsDeleted = false
            };

            var mystery = new Category
            {
                Id = 2,
                Title = "Mystery",
                Description = "Mystery stories",
                IsDeleted = false
            };

            var thriller = new Category
            {
                Id = 3,
                Title = "Thriller",
                Description = "Thriller stories",
                IsDeleted = false
            };

            var deletedCategory = new Category
            {
                Id = 4,
                Title = "Deleted Category",
                Description = "Deleted category",
                IsDeleted = true
            };

            await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
            await context.Categories.AddRangeAsync(horror, mystery, thriller, deletedCategory);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var stories = new List<Story>
            {
                new Story { Id = Guid.NewGuid(), Title = "H1", Content = "Content", AuthorId = user.Id, Author = user, CategoryId = horror.Id, Category = horror, CreatedOn = DateTime.UtcNow, IsDeleted = false },
                new Story { Id = Guid.NewGuid(), Title = "H2", Content = "Content", AuthorId = user.Id, Author = user, CategoryId = horror.Id, Category = horror, CreatedOn = DateTime.UtcNow, IsDeleted = false },
                new Story { Id = Guid.NewGuid(), Title = "H3", Content = "Content", AuthorId = user.Id, Author = user, CategoryId = horror.Id, Category = horror, CreatedOn = DateTime.UtcNow, IsDeleted = false },

                new Story { Id = Guid.NewGuid(), Title = "M1", Content = "Content", AuthorId = user.Id, Author = user, CategoryId = mystery.Id, Category = mystery, CreatedOn = DateTime.UtcNow, IsDeleted = false },
                new Story { Id = Guid.NewGuid(), Title = "M2", Content = "Content", AuthorId = user.Id, Author = user, CategoryId = mystery.Id, Category = mystery, CreatedOn = DateTime.UtcNow, IsDeleted = false },

                new Story { Id = Guid.NewGuid(), Title = "T1", Content = "Content", AuthorId = user.Id, Author = user, CategoryId = thriller.Id, Category = thriller, CreatedOn = DateTime.UtcNow, IsDeleted = false },

                new Story { Id = Guid.NewGuid(), Title = "D1", Content = "Content", AuthorId = user.Id, Author = user, CategoryId = deletedCategory.Id, Category = deletedCategory, CreatedOn = DateTime.UtcNow, IsDeleted = false }
            };

            await context.Stories.AddRangeAsync(stories, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            HomeIndexDto result = await service.GetHomePageDataAsync();

            result.FeaturedCategories.Should().HaveCount(3);
            result.FeaturedCategories.Select(c => c.Title).Should()
                .ContainInOrder("Horror", "Mystery", "Thriller");
            result.FeaturedCategories.Select(c => c.Title).Should().NotContain("Deleted Category");
        }

        [Fact]
        public async Task GetHomePageDataAsync_ShouldReturnOnlyUpcomingNonDeletedEvents_OrderedByStartDate()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            HomeService service = CreateService(context);

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "testuser",
                Email = "test@test.com"
            };

            var earliestEvent = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Earliest Event",
                Description = "Event description",
                CreatorId = user.Id,
                Creator = user,
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(2).AddHours(2),
                IsDeleted = false
            };

            var laterEvent = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Later Event",
                Description = "Event description",
                CreatorId = user.Id,
                Creator = user,
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(5).AddHours(2),
                IsDeleted = false
            };

            var latestEvent = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Latest Event",
                Description = "Event description",
                CreatorId = user.Id,
                Creator = user,
                StartDate = DateTime.UtcNow.AddDays(8),
                EndDate = DateTime.UtcNow.AddDays(8).AddHours(2),
                IsDeleted = false
            };

            var pastEvent = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Past Event",
                Description = "Event description",
                CreatorId = user.Id,
                Creator = user,
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(-2).AddHours(2),
                IsDeleted = false
            };

            var deletedEvent = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Deleted Event",
                Description = "Event description",
                CreatorId = user.Id,
                Creator = user,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                IsDeleted = true
            };

            await context.Users.AddAsync(user, TestContext.Current.CancellationToken);
            await context.Events.AddRangeAsync(earliestEvent, laterEvent, latestEvent, pastEvent, deletedEvent);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            HomeIndexDto result = await service.GetHomePageDataAsync();

            result.FeaturedEvents.Should().HaveCount(3);
            result.FeaturedEvents.Select(e => e.Title).Should()
                .ContainInOrder("Earliest Event", "Later Event", "Latest Event");
            result.FeaturedEvents.Select(e => e.Title).Should().NotContain("Past Event");
            result.FeaturedEvents.Select(e => e.Title).Should().NotContain("Deleted Event");
        }

        [Fact]
        public async Task GetHomePageDataAsync_ShouldTakeOnlyTopThreeCategories_AndTopThreeEvents_AndTopTenStories()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            HomeService service = CreateService(context);

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "testuser",
                Email = "test@test.com"
            };

            await context.Users.AddAsync(user, TestContext.Current.CancellationToken);

            for (int i = 1; i <= 5; i++)
            {
                await context.Categories.AddAsync(new Category
                {
                    Id = i,
                    Title = $"Category {i}",
                    Description = $"Description {i}",
                    IsDeleted = false
                }, TestContext.Current.CancellationToken);
            }

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var stories = new List<Story>();
            for (int i = 1; i <= 12; i++)
            {
                stories.Add(new Story
                {
                    Id = Guid.NewGuid(),
                    Title = $"Story {i}",
                    Content = $"Content {i}",
                    AuthorId = user.Id,
                    Author = user,
                    CategoryId = 1,
                    CreatedOn = DateTime.UtcNow.AddDays(-i),
                    ViewsCount = i,
                    LikesCount = i,
                    IsDeleted = false
                });
            }

            var eventsList = new List<Event>();
            for (int i = 1; i <= 5; i++)
            {
                eventsList.Add(new Event
                {
                    Id = Guid.NewGuid(),
                    Title = $"Event {i}",
                    Description = $"Description {i}",
                    CreatorId = user.Id,
                    Creator = user,
                    StartDate = DateTime.UtcNow.AddDays(i),
                    EndDate = DateTime.UtcNow.AddDays(i).AddHours(2),
                    IsDeleted = false
                });
            }

            await context.Stories.AddRangeAsync(stories, TestContext.Current.CancellationToken);
            await context.Events.AddRangeAsync(eventsList, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            HomeIndexDto result = await service.GetHomePageDataAsync();

            result.TopStoriesOfMonth.Should().HaveCount(10);
            result.LatestStories.Should().HaveCount(10);
            result.FeaturedCategories.Should().HaveCount(3);
            result.FeaturedEvents.Should().HaveCount(3);
        }
    }
}