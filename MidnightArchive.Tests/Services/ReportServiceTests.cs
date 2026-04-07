using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.DTOs.ReportDTOs;
using MidnightArchive.Core.Services;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Enums;
using MidnightArchive.Infra.Data.Models;
using MidnightArchive.Tests.Helpers;

namespace MidnightArchive.Tests.Services
{
    public class ReportServiceTests
    {
        private ReportService CreateService(ApplicationDbContext context)
            => new ReportService(context);

        [Fact]
        public async Task CreateAsync_ShouldReturnStoryNotFound_WhenStoryDoesNotExist()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            ReportService service = CreateService(context);

            var model = new ReportCreateDto
            {
                StoryId = Guid.NewGuid(),
                Reason = ReportReason.Spam,
                Description = "Spam content"
            };

            ReportOperationResult result = await service.CreateAsync(model, "user-1");

            result.Should().Be(ReportOperationResult.StoryNotFound);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnStoryNotFound_WhenStoryIsDeleted()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            ReportService service = CreateService(context);

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

            var model = new ReportCreateDto
            {
                StoryId = story.Id,
                Reason = ReportReason.OffensiveContent,
                Description = "Offensive content"
            };

            ReportOperationResult result = await service.CreateAsync(model, "user-1");

            result.Should().Be(ReportOperationResult.StoryNotFound);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnAlreadyReported_WhenUserAlreadyHasActiveReportForStory()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            ReportService service = CreateService(context);

            var story = new Story
            {
                Id = Guid.NewGuid(),
                Title = "Story",
                Content = "Story content",
                AuthorId = "author-1",
                CategoryId = 1,
                IsDeleted = false
            };

            var existingReport = new Report
            {
                Id = Guid.NewGuid(),
                StoryId = story.Id,
                ReporterId = "user-1",
                Reason = ReportReason.Spam,
                Description = "Existing report",
                CreatedOn = DateTime.UtcNow.AddDays(-1),
                IsResolved = false
            };

            await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
            await context.Reports.AddAsync(existingReport, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var model = new ReportCreateDto
            {
                StoryId = story.Id,
                Reason = ReportReason.Spam,
                Description = "New report attempt"
            };

            ReportOperationResult result = await service.CreateAsync(model, "user-1");

            result.Should().Be(ReportOperationResult.AlreadyReported);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateReport_WhenStoryExistsAndUserHasNotReportedIt()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            ReportService service = CreateService(context);

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

            var model = new ReportCreateDto
            {
                StoryId = story.Id,
                Reason = ReportReason.Harassment,
                Description = "Report description"
            };

            ReportOperationResult result = await service.CreateAsync(model, "user-1");

            Report? reportInDb = await context.Reports
                .FirstOrDefaultAsync(r => r.StoryId == story.Id && r.ReporterId == "user-1", TestContext.Current.CancellationToken);

            result.Should().Be(ReportOperationResult.Success);
            reportInDb.Should().NotBeNull();
            reportInDb!.Reason.Should().Be(ReportReason.Harassment);
            reportInDb.Description.Should().Be("Report description");
            reportInDb.IsResolved.Should().BeFalse();
            reportInDb.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateNewReport_WhenPreviousReportIsResolved()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            ReportService service = CreateService(context);

            var story = new Story
            {
                Id = Guid.NewGuid(),
                Title = "Story",
                Content = "Story content",
                AuthorId = "author-1",
                CategoryId = 1,
                IsDeleted = false
            };

            var resolvedReport = new Report
            {
                Id = Guid.NewGuid(),
                StoryId = story.Id,
                ReporterId = "user-1",
                Reason = ReportReason.Spam,
                Description = "Old resolved report",
                CreatedOn = DateTime.UtcNow.AddDays(-2),
                IsResolved = true
            };

            await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
            await context.Reports.AddAsync(resolvedReport, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var model = new ReportCreateDto
            {
                StoryId = story.Id,
                Reason = ReportReason.Other,
                Description = "New active report"
            };

            ReportOperationResult result = await service.CreateAsync(model, "user-1");

            List<Report> reports = await context.Reports
                .Where(r => r.StoryId == story.Id && r.ReporterId == "user-1")
                .ToListAsync(TestContext.Current.CancellationToken);

            result.Should().Be(ReportOperationResult.Success);
            reports.Should().HaveCount(2);
            reports.Should().Contain(r => !r.IsResolved && r.Description == "New active report");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyCollection_WhenThereAreNoReports()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            ReportService service = CreateService(context);

            IEnumerable<ReportListDto> result = await service.GetAllAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnReportsOrderedByCreatedOnDescending()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            ReportService service = CreateService(context);

            var reporter = new ApplicationUser
            {
                Id = "user-1",
                UserName = "reporter",
                Email = "reporter@test.com"
            };

            var story = new Story
            {
                Id = Guid.NewGuid(),
                Title = "Reported story",
                Content = "Story content",
                AuthorId = "author-1",
                CategoryId = 1,
                IsDeleted = false
            };

            var olderReport = new Report
            {
                Id = Guid.NewGuid(),
                StoryId = story.Id,
                Story = story,
                ReporterId = reporter.Id,
                Reporter = reporter,
                Reason = ReportReason.Spam,
                Description = "Older report",
                CreatedOn = DateTime.UtcNow.AddHours(-3),
                IsResolved = false
            };

            var newerReport = new Report
            {
                Id = Guid.NewGuid(),
                StoryId = story.Id,
                Story = story,
                ReporterId = reporter.Id,
                Reporter = reporter,
                Reason = ReportReason.Harassment,
                Description = "Newer report",
                CreatedOn = DateTime.UtcNow.AddHours(-1),
                IsResolved = true
            };

            await context.Users.AddAsync(reporter, TestContext.Current.CancellationToken);
            await context.Stories.AddAsync(story, TestContext.Current.CancellationToken);
            await context.Reports.AddRangeAsync(olderReport, newerReport);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            List<ReportListDto> result = (await service.GetAllAsync()).ToList();

            result.Should().HaveCount(2);
            result[0].Id.Should().Be(newerReport.Id);
            result[0].StoryTitle.Should().Be("Reported story");
            result[0].ReporterName.Should().Be("reporter");
            result[0].Reason.Should().Be(ReportReason.Harassment);
            result[0].IsResolved.Should().BeTrue();

            result[1].Id.Should().Be(olderReport.Id);
            result[1].Reason.Should().Be(ReportReason.Spam);
            result[1].IsResolved.Should().BeFalse();
        }

        [Fact]
        public async Task ResolveAsync_ShouldReturnReportNotFound_WhenReportDoesNotExist()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            ReportService service = CreateService(context);

            ReportOperationResult result = await service.ResolveAsync(Guid.NewGuid());

            result.Should().Be(ReportOperationResult.ReportNotFound);
        }

        [Fact]
        public async Task ResolveAsync_ShouldMarkReportAsResolved_WhenReportExists()
        {
            using ApplicationDbContext context = TestDbContextFactory.Create();
            ReportService service = CreateService(context);

            var report = new Report
            {
                Id = Guid.NewGuid(),
                StoryId = Guid.NewGuid(),
                ReporterId = "user-1",
                Reason = ReportReason.FalseInformation,
                Description = "Report description",
                CreatedOn = DateTime.UtcNow.AddHours(-1),
                IsResolved = false
            };

            await context.Reports.AddAsync(report, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            ReportOperationResult result = await service.ResolveAsync(report.Id);

            Report? updatedReport = await context.Reports.FirstOrDefaultAsync(r => r.Id == report.Id, TestContext.Current.CancellationToken);

            result.Should().Be(ReportOperationResult.Success);
            updatedReport.Should().NotBeNull();
            updatedReport!.IsResolved.Should().BeTrue();
        }
    }
}