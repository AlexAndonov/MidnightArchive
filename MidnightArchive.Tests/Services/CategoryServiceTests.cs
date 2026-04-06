using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Core.DTOs.CategoryDTOs;
using MidnightArchive.Core.Services;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Enums;
using MidnightArchive.Infra.Data.Models;
using MidnightArchive.Tests.Helpers;

namespace MidnightArchive.Tests.Services
{
	public class CategoryServiceTests
	{
		private ApplicationDbContext CreateDbContext()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

			return new ApplicationDbContext(options);
		}

		private static IMapper CreateMapper()
			=> MapperFactory.Create();

		private CategoryService CreateService(ApplicationDbContext context)
			=> new CategoryService(context, CreateMapper());

		[Fact]
		public async Task AddAsync_ShouldAddCategoryAndReturnMappedDto()
		{
			using var context = CreateDbContext();
			var service = CreateService(context);

			var model = new CategoryCreateDto
			{
				Title = "Horror",
				Description = "Dark horror stories"
			};

			var result = await service.AddAsync(model);

			result.Should().NotBeNull();
			result.Id.Should().BeGreaterThan(0);
			result.Title.Should().Be("Horror");
			result.Description.Should().Be("Dark horror stories");

			var categoryInDb = await context.Categories.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
			categoryInDb.Should().NotBeNull();
			categoryInDb!.Title.Should().Be("Horror");
			categoryInDb.Description.Should().Be("Dark horror stories");
			categoryInDb.IsDeleted.Should().BeFalse();
		}

		[Fact]
		public async Task EditAsync_ShouldReturnNotFound_WhenCategoryDoesNotExist()
		{
			using var context = CreateDbContext();
			var service = CreateService(context);

			var model = new CategoryEditDto
			{
				Id = 999,
				Title = "Updated",
				Description = "Updated description"
			};

			var result = await service.EditAsync(model);

			result.Should().Be(CategoryOperationResult.NotFound);
		}

		[Fact]
		public async Task EditAsync_ShouldReturnNotFound_WhenCategoryIsSoftDeleted()
		{
			using var context = CreateDbContext();

			context.Categories.Add(new Category
			{
				Id = 1,
				Title = "Old",
				Description = "Old description",
				IsDeleted = true
			});
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var service = CreateService(context);

			var model = new CategoryEditDto
			{
				Id = 1,
				Title = "Updated",
				Description = "Updated description"
			};

			var result = await service.EditAsync(model);

			result.Should().Be(CategoryOperationResult.NotFound);
		}

		[Fact]
		public async Task EditAsync_ShouldUpdateCategoryAndReturnSuccess_WhenCategoryExists()
		{
			using var context = CreateDbContext();

			context.Categories.Add(new Category
			{
				Id = 1,
				Title = "Old Title",
				Description = "Old Description"
			});
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var service = CreateService(context);

			var model = new CategoryEditDto
			{
				Id = 1,
				Title = "New Title",
				Description = "New Description"
			};

			var result = await service.EditAsync(model);

			result.Should().Be(CategoryOperationResult.Success);

			var categoryInDb = await context.Categories.FirstAsync(c => c.Id == 1, TestContext.Current.CancellationToken);
			categoryInDb.Title.Should().Be("New Title");
			categoryInDb.Description.Should().Be("New Description");
		}

		[Fact]
		public async Task SoftDeleteAsync_ShouldReturnNotFound_WhenCategoryDoesNotExist()
		{
			using var context = CreateDbContext();
			var service = CreateService(context);

			var result = await service.SoftDeleteAsync(123);

			result.Should().Be(CategoryOperationResult.NotFound);
		}

		[Fact]
		public async Task SoftDeleteAsync_ShouldReturnNotFound_WhenCategoryAlreadyDeleted()
		{
			using var context = CreateDbContext();

			context.Categories.Add(new Category
			{
				Id = 1,
				Title = "Deleted Category",
				Description = "Description",
				IsDeleted = true
			});
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var service = CreateService(context);

			var result = await service.SoftDeleteAsync(1);

			result.Should().Be(CategoryOperationResult.NotFound);
		}

		[Fact]
		public async Task SoftDeleteAsync_ShouldMarkCategoryAsDeletedAndReturnSuccess()
		{
			using var context = CreateDbContext();

			context.Categories.Add(new Category
			{
				Id = 1,
				Title = "Category",
				Description = "Description",
				IsDeleted = false
			});
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var service = CreateService(context);

			var result = await service.SoftDeleteAsync(1);

			result.Should().Be(CategoryOperationResult.Success);

			var categoryInDb = await context.Categories.FirstAsync(c => c.Id == 1, TestContext.Current.CancellationToken);
			categoryInDb.IsDeleted.Should().BeTrue();
		}

		[Fact]
		public async Task HardDeleteAsync_ShouldReturnNotFound_WhenCategoryDoesNotExist()
		{
			using var context = CreateDbContext();
			var service = CreateService(context);

			var result = await service.HardDeleteAsync(999);

			result.Should().Be(CategoryOperationResult.NotFound);
		}

		[Fact]
		public async Task HardDeleteAsync_ShouldRemoveCategoryAndReturnSuccess_WhenCategoryExists()
		{
			using var context = CreateDbContext();

			context.Categories.Add(new Category
			{
				Id = 1,
				Title = "To Delete",
				Description = "Description"
			});
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var service = CreateService(context);

			var result = await service.HardDeleteAsync(1);

			result.Should().Be(CategoryOperationResult.Success);

			var categoryInDb = await context.Categories.FirstOrDefaultAsync(c => c.Id == 1, TestContext.Current.CancellationToken);
			categoryInDb.Should().BeNull();
		}

		[Fact]
		public async Task GetAllAsync_ShouldReturnOnlyNonDeletedCategoriesOrderedByStoriesCount()
		{
			using var context = CreateDbContext();

			var user = new ApplicationUser
			{
				Id = "user-1",
				UserName = "testuser",
				Email = "test@test.com"
			};

			var category1 = new Category
			{
				Id = 1,
				Title = "Category 1",
				Description = "Description 1",
				IsDeleted = false
			};

			var category2 = new Category
			{
				Id = 2,
				Title = "Category 2",
				Description = "Description 2",
				IsDeleted = false
			};

			var deletedCategory = new Category
			{
				Id = 3,
				Title = "Deleted Category",
				Description = "Deleted Description",
				IsDeleted = true
			};

			context.Users.Add(user);
			context.Categories.AddRange(category1, category2, deletedCategory);

			context.Stories.AddRange(
				new Story
				{
					Id = Guid.NewGuid(),
					Title = "Story 1",
					Content = "Content 1",
					AuthorId = user.Id,
					Author = user,
					CategoryId = 1,
					Category = category1,
					IsDeleted = false,
					CreatedOn = DateTime.UtcNow
				},
				new Story
				{
					Id = Guid.NewGuid(),
					Title = "Story 2",
					Content = "Content 2",
					AuthorId = user.Id,
					Author = user,
					CategoryId = 1,
					Category = category1,
					IsDeleted = false,
					CreatedOn = DateTime.UtcNow.AddMinutes(-1)
				},
				new Story
				{
					Id = Guid.NewGuid(),
					Title = "Story 3",
					Content = "Content 3",
					AuthorId = user.Id,
					Author = user,
					CategoryId = 2,
					Category = category2,
					IsDeleted = false,
					CreatedOn = DateTime.UtcNow.AddMinutes(-2)
				},
				new Story
				{
					Id = Guid.NewGuid(),
					Title = "Deleted Story",
					Content = "Deleted content",
					AuthorId = user.Id,
					Author = user,
					CategoryId = 2,
					Category = category2,
					IsDeleted = true,
					CreatedOn = DateTime.UtcNow.AddMinutes(-3)
				}
			);

			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var service = CreateService(context);

			var result = (await service.GetAllAsync()).ToList();

			result.Should().HaveCount(2);
			result[0].Id.Should().Be(1);
			result[0].StoriesCount.Should().Be(2);

			result[1].Id.Should().Be(2);
			result[1].StoriesCount.Should().Be(1);

			result.Should().NotContain(c => c.Id == 3);
		}

		[Fact]
		public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
		{
			using var context = CreateDbContext();
			var service = CreateService(context);

			var result = await service.GetByIdAsync(999);

			result.Should().BeNull();
		}

		[Fact]
		public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryIsDeleted()
		{
			using var context = CreateDbContext();

			context.Categories.Add(new Category
			{
				Id = 1,
				Title = "Deleted",
				Description = "Description",
				IsDeleted = true
			});
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var service = CreateService(context);

			var result = await service.GetByIdAsync(1);

			result.Should().BeNull();
		}

		[Fact]
		public async Task GetByIdAsync_ShouldReturnCategoryWithOnlyNonDeletedStories()
		{
			using var context = CreateDbContext();

			var user = new ApplicationUser
			{
				Id = "user-1",
				UserName = "ivan",
				Email = "ivan@test.com"
			};

			var category = new Category
			{
				Id = 1,
				Title = "Horror",
				Description = "Scary stories"
			};

			var longContent = new string('A', 120);

			context.Users.Add(user);
			context.Categories.Add(category);

			context.Stories.AddRange(
				new Story
				{
					Id = Guid.NewGuid(),
					Title = "Newest Story",
					Content = longContent,
					CreatedOn = new DateTime(2025, 1, 2),
					AuthorId = user.Id,
					Author = user,
					CategoryId = category.Id,
					Category = category,
					ViewsCount = 100,
					LikesCount = 10,
					IsAnonymous = false,
					IsDeleted = false
				},
				new Story
				{
					Id = Guid.NewGuid(),
					Title = "Deleted Story",
					Content = "Should not appear",
					CreatedOn = new DateTime(2025, 1, 1),
					AuthorId = user.Id,
					Author = user,
					CategoryId = category.Id,
					Category = category,
					IsDeleted = true
				}
			);

			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var service = CreateService(context);

			var result = await service.GetByIdAsync(1);

			result.Should().NotBeNull();
			result!.Id.Should().Be(1);
			result.Title.Should().Be("Horror");
			result.Description.Should().Be("Scary stories");

			result.Stories.Should().NotBeNull();
			result.Stories.TotalCount.Should().Be(1);
			result.Stories.Page.Should().Be(1);
			result.Stories.PageSize.Should().Be(1);

			result.Stories.Items.Should().HaveCount(1);

			var story = result.Stories.Items.First();
			story.Title.Should().Be("Newest Story");
			story.AuthorName.Should().Be("ivan");
			story.ViewsCount.Should().Be(100);
			story.LikesCount.Should().Be(10);
			story.IsAnonymous.Should().BeFalse();
			story.Preview.Should().HaveLength(103);
			story.Preview.Should().EndWith("...");
		}

		[Fact]
		public async Task GetByIdAsync_WithPaging_ShouldReturnNull_WhenCategoryDoesNotExist()
		{
			using var context = CreateDbContext();
			var service = CreateService(context);

			var result = await service.GetByIdAsync(555, 1, 10);

			result.Should().BeNull();
		}

		[Fact]
		public async Task GetByIdAsync_WithPaging_ShouldNormalizeInvalidPageAndPageSize()
		{
			using var context = CreateDbContext();

			var user = new ApplicationUser
			{
				Id = "user-1",
				UserName = "john",
				Email = "john@test.com"
			};

			var category = new Category
			{
				Id = 1,
				Title = "Mystery",
				Description = "Mystery stories"
			};

			context.Users.Add(user);
			context.Categories.Add(category);

			context.Stories.Add(new Story
			{
				Id = Guid.NewGuid(),
				Title = "Story 1",
				Content = "Short content",
				CreatedOn = new DateTime(2025, 1, 1),
				AuthorId = user.Id,
				Author = user,
				CategoryId = category.Id,
				Category = category,
				IsDeleted = false,
				IsAnonymous = false
			});

			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var service = CreateService(context);

			var result = await service.GetByIdAsync(1, 0, 0);

			result.Should().NotBeNull();
			result!.Stories.Page.Should().Be(1);
			result.Stories.PageSize.Should().Be(10);
			result.Stories.TotalCount.Should().Be(1);
			result.Stories.Items.Should().HaveCount(1);
		}

		[Fact]
		public async Task GetByIdAsync_WithPaging_ShouldReturnCorrectPagedStoriesOrderedByCreatedOnDescending()
		{
			using var context = CreateDbContext();

			var user = new ApplicationUser
			{
				Id = "user-1",
				UserName = "george",
				Email = "george@test.com"
			};

			var category = new Category
			{
				Id = 1,
				Title = "Thriller",
				Description = "Thriller stories"
			};

			context.Users.Add(user);
			context.Categories.Add(category);

			context.Stories.AddRange(
				new Story
				{
					Id = Guid.NewGuid(),
					Title = "Oldest",
					Content = "Oldest content",
					CreatedOn = new DateTime(2025, 1, 1),
					AuthorId = user.Id,
					Author = user,
					CategoryId = 1,
					Category = category,
					IsDeleted = false,
					IsAnonymous = false,
					ViewsCount = 1,
					LikesCount = 1
				},
				new Story
				{
					Id = Guid.NewGuid(),
					Title = "Middle",
					Content = "Middle content",
					CreatedOn = new DateTime(2025, 1, 2),
					AuthorId = user.Id,
					Author = user,
					CategoryId = 1,
					Category = category,
					IsDeleted = false,
					IsAnonymous = true,
					ViewsCount = 2,
					LikesCount = 2
				},
				new Story
				{
					Id = Guid.NewGuid(),
					Title = "Newest",
					Content = "Newest content",
					CreatedOn = new DateTime(2025, 1, 3),
					AuthorId = user.Id,
					Author = user,
					CategoryId = 1,
					Category = category,
					IsDeleted = false,
					IsAnonymous = false,
					ViewsCount = 3,
					LikesCount = 3
				}
			);

			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var service = CreateService(context);

			var result = await service.GetByIdAsync(1, 2, 1);

			result.Should().NotBeNull();
			result!.Id.Should().Be(1);
			result.Title.Should().Be("Thriller");
			result.Description.Should().Be("Thriller stories");

			result.Stories.TotalCount.Should().Be(3);
			result.Stories.Page.Should().Be(2);
			result.Stories.PageSize.Should().Be(1);
			result.Stories.Items.Should().HaveCount(1);

			var story = result.Stories.Items.Single();
			story.Title.Should().Be("Middle");
			story.AuthorName.Should().Be("Anonymous");
			story.IsAnonymous.Should().BeTrue();
			story.ViewsCount.Should().Be(2);
			story.LikesCount.Should().Be(2);
		}

		[Fact]
		public async Task GetForEditAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
		{
			using var context = CreateDbContext();
			var service = CreateService(context);

			var result = await service.GetForEditAsync(999);

			result.Should().BeNull();
		}

		[Fact]
		public async Task GetForEditAsync_ShouldReturnNull_WhenCategoryIsDeleted()
		{
			using var context = CreateDbContext();

			context.Categories.Add(new Category
			{
				Id = 1,
				Title = "Deleted",
				Description = "Description",
				IsDeleted = true
			});
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var service = CreateService(context);

			var result = await service.GetForEditAsync(1);

			result.Should().BeNull();
		}

		[Fact]
		public async Task GetForEditAsync_ShouldReturnMappedCategoryEditDto_WhenCategoryExists()
		{
			using var context = CreateDbContext();

			context.Categories.Add(new Category
			{
				Id = 1,
				Title = "Editable Title",
				Description = "Editable Description",
				IsDeleted = false
			});
			await context.SaveChangesAsync(TestContext.Current.CancellationToken);

			var service = CreateService(context);

			var result = await service.GetForEditAsync(1);

			result.Should().NotBeNull();
			result!.Id.Should().Be(1);
			result.Title.Should().Be("Editable Title");
			result.Description.Should().Be("Editable Description");
		}
	}
}