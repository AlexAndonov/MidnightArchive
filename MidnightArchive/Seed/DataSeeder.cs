using Microsoft.EntityFrameworkCore;
using MidnightArchive.Data;
using MidnightArchive.Infra.Data.Models;

namespace MidnightArchive.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Categories.AnyAsync() || await context.Stories.AnyAsync())
            {
                return;
            }

            const string adminEmail = "admin@midnight.com";
            const string userEmail = "user@midnight.com";
            const string writerEmail = "writer@midnight.com";

            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            var normalUser = await context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            var writerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == writerEmail);

            if (adminUser == null || normalUser == null || writerUser == null)
            {
                return;
            }

            // Categories
            var horror = new Category
            {
                Title = "Horror",
                Description = "Dark and terrifying stories filled with fear, dread, and the unknown."
            };

            var mystery = new Category
            {
                Title = "Mystery",
                Description = "Stories about secrets, disappearances, strange clues, and hidden truths."
            };

            var thriller = new Category
            {
                Title = "Thriller",
                Description = "Fast-paced suspense stories with tension, danger, and unexpected twists."
            };

            var supernatural = new Category
            {
                Title = "Supernatural",
                Description = "Ghosts, curses, visions, haunted places, and events beyond explanation."
            };

            var psychological = new Category
            {
                Title = "Psychological",
                Description = "Stories exploring paranoia, obsession, mental breakdown, and inner fear."
            };

            await context.Categories.AddRangeAsync(horror, mystery, thriller, supernatural, psychological);
            await context.SaveChangesAsync();

            var stories = new List<Story>();

            // Horror - 25 stories for pagination demo
            for (int i = 1; i <= 25; i++)
            {
                stories.Add(new Story
                {
                    Id = Guid.NewGuid(),
                    Title = $"Midnight Horror Tale #{i}",
                    Content = $"At exactly 3:07 AM, the hallway light turned on by itself. " +
                              $"I thought it was a power fluctuation, until I saw the wet footprints " +
                              $"leading from the front door to my bedroom. They stopped right beside my bed. " +
                              $"The next morning, the door was still locked from the inside. This is horror story number {i}.",
                    CreatedOn = DateTime.UtcNow.AddDays(-i),
                    AuthorId = i % 2 == 0 ? writerUser.Id : normalUser.Id,
                    CategoryId = horror.Id,
                    ViewsCount = 20 + i * 3,
                    LikesCount = 5 + i,
                    IsAnonymous = i % 4 == 0
                });
            }

            // Mystery
            stories.AddRange(new[]
            {
                new Story
                {
                    Id = Guid.NewGuid(),
                    Title = "The Missing Train Ticket",
                    Content = "My grandfather kept a train ticket in a locked drawer for forty years. " +
                              "When he died, I found the ticket and realized the destination was a town that no longer exists on any map.",
                    CreatedOn = DateTime.UtcNow.AddDays(-3),
                    AuthorId = normalUser.Id,
                    CategoryId = mystery.Id,
                    ViewsCount = 84,
                    LikesCount = 17,
                    IsAnonymous = false
                },
                new Story
                {
                    Id = Guid.NewGuid(),
                    Title = "A Letter with No Sender",
                    Content = "Every month, an old envelope arrives at my apartment. " +
                              "No stamp, no sender, no fingerprints. Inside there is always one sentence describing something I will experience the next day.",
                    CreatedOn = DateTime.UtcNow.AddDays(-7),
                    AuthorId = writerUser.Id,
                    CategoryId = mystery.Id,
                    ViewsCount = 63,
                    LikesCount = 12,
                    IsAnonymous = false
                },
                new Story
                {
                    Id = Guid.NewGuid(),
                    Title = "The Apartment Across the Street",
                    Content = "The woman in the apartment across from mine performs the exact same movements every night at 11:14 PM. " +
                              "One night she stopped, looked directly at me, and turned off the light.",
                    CreatedOn = DateTime.UtcNow.AddDays(-9),
                    AuthorId = normalUser.Id,
                    CategoryId = mystery.Id,
                    ViewsCount = 51,
                    LikesCount = 11,
                    IsAnonymous = true
                }
            });

            // Thriller
            stories.AddRange(new[]
            {
                new Story
                {
                    Id = Guid.NewGuid(),
                    Title = "Locked Inside",
                    Content = "I woke up in a hotel room with no memory of how I got there. " +
                              "The windows were sealed shut, the phone line was dead, and someone had written 'DON'T OPEN THE BATHROOM DOOR' on the mirror.",
                    CreatedOn = DateTime.UtcNow.AddDays(-5),
                    AuthorId = writerUser.Id,
                    CategoryId = thriller.Id,
                    ViewsCount = 97,
                    LikesCount = 29,
                    IsAnonymous = false
                },
                new Story
                {
                    Id = Guid.NewGuid(),
                    Title = "The Last Voice Message",
                    Content = "My sister sent me a voice message saying someone was following her. " +
                              "Halfway through the recording, she stopped speaking and another voice quietly said my name.",
                    CreatedOn = DateTime.UtcNow.AddDays(-11),
                    AuthorId = normalUser.Id,
                    CategoryId = thriller.Id,
                    ViewsCount = 76,
                    LikesCount = 18,
                    IsAnonymous = false
                },
                new Story
                {
                    Id = Guid.NewGuid(),
                    Title = "Someone Knew My Route",
                    Content = "I changed my route home every night for a week. " +
                              "Every night, the same black car was parked exactly where I would pass.",
                    CreatedOn = DateTime.UtcNow.AddDays(-13),
                    AuthorId = writerUser.Id,
                    CategoryId = thriller.Id,
                    ViewsCount = 44,
                    LikesCount = 9,
                    IsAnonymous = true
                }
            });

            // Supernatural
            stories.AddRange(new[]
            {
                new Story
                {
                    Id = Guid.NewGuid(),
                    Title = "The Third Floor Bell",
                    Content = "The abandoned hospital near my village still has power somehow. " +
                              "Every Sunday night, a bell rings on the third floor where no one has been allowed since 1986.",
                    CreatedOn = DateTime.UtcNow.AddDays(-6),
                    AuthorId = normalUser.Id,
                    CategoryId = supernatural.Id,
                    ViewsCount = 88,
                    LikesCount = 20,
                    IsAnonymous = false
                },
                new Story
                {
                    Id = Guid.NewGuid(),
                    Title = "My Reflection Blinked",
                    Content = "I was brushing my teeth when my reflection blinked half a second later than I did. " +
                              "I laughed at first, until it smiled and I didn't.",
                    CreatedOn = DateTime.UtcNow.AddDays(-15),
                    AuthorId = writerUser.Id,
                    CategoryId = supernatural.Id,
                    ViewsCount = 120,
                    LikesCount = 34,
                    IsAnonymous = false
                },
                new Story
                {
                    Id = Guid.NewGuid(),
                    Title = "The House That Remembers",
                    Content = "My grandmother's house knows when someone lies. " +
                              "The walls creak, the radio switches on, and a cold draft crawls under the doors.",
                    CreatedOn = DateTime.UtcNow.AddDays(-18),
                    AuthorId = normalUser.Id,
                    CategoryId = supernatural.Id,
                    ViewsCount = 59,
                    LikesCount = 15,
                    IsAnonymous = true
                }
            });

            // Psychological
            stories.AddRange(new[]
            {
                new Story
                {
                    Id = Guid.NewGuid(),
                    Title = "The Notebook I Never Bought",
                    Content = "I found a notebook on my desk full of entries written in my handwriting. " +
                              "The strange part was that every entry described thoughts I had never said aloud to anyone.",
                    CreatedOn = DateTime.UtcNow.AddDays(-4),
                    AuthorId = writerUser.Id,
                    CategoryId = psychological.Id,
                    ViewsCount = 72,
                    LikesCount = 21,
                    IsAnonymous = false
                },
                new Story
                {
                    Id = Guid.NewGuid(),
                    Title = "Do Not Trust the Elevator Camera",
                    Content = "Every morning I reviewed the security feed from our building. " +
                              "For a week, the camera showed me entering the elevator twice, but only one of me ever came out.",
                    CreatedOn = DateTime.UtcNow.AddDays(-8),
                    AuthorId = normalUser.Id,
                    CategoryId = psychological.Id,
                    ViewsCount = 101,
                    LikesCount = 26,
                    IsAnonymous = false
                },
                new Story
                {
                    Id = Guid.NewGuid(),
                    Title = "I Heard My Thoughts Answer Back",
                    Content = "The first time it happened, I assumed I was exhausted. " +
                              "The second time, the answer came before I finished thinking the question.",
                    CreatedOn = DateTime.UtcNow.AddDays(-14),
                    AuthorId = writerUser.Id,
                    CategoryId = psychological.Id,
                    ViewsCount = 67,
                    LikesCount = 14,
                    IsAnonymous = true
                }
            });

            await context.Stories.AddRangeAsync(stories);
            await context.SaveChangesAsync();

            // Events
            var events = new List<Event>
            {
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Night Horror Writing Session",
                    Description = "A late-night online event where writers share and discuss original horror stories.",
                    Location = "Online",
                    CreatorId = writerUser.Id,
                    StartDate = DateTime.UtcNow.AddDays(5).Date.AddHours(20),
                    EndDate = DateTime.UtcNow.AddDays(5).Date.AddHours(22),
                    IsDeleted = false
                },
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Mystery Readers Meetup",
                    Description = "A community event for mystery fans to discuss unresolved endings, clues, and favorite authors.",
                    Location = "Sofia",
                    CreatorId = normalUser.Id,
                    StartDate = DateTime.UtcNow.AddDays(9).Date.AddHours(18),
                    EndDate = DateTime.UtcNow.AddDays(9).Date.AddHours(20),
                    IsDeleted = false
                },
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Psychological Suspense Workshop",
                    Description = "A focused workshop on building tension, unreliable narrators, and mind-bending twists.",
                    Location = "Plovdiv",
                    CreatorId = writerUser.Id,
                    StartDate = DateTime.UtcNow.AddDays(14).Date.AddHours(19),
                    EndDate = DateTime.UtcNow.AddDays(14).Date.AddHours(21),
                    IsDeleted = false
                },
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Supernatural Story Night",
                    Description = "A themed evening dedicated to ghost stories, folklore, curses, and paranormal fiction.",
                    Location = "Online",
                    CreatorId = normalUser.Id,
                    StartDate = DateTime.UtcNow.AddDays(20).Date.AddHours(20),
                    EndDate = DateTime.UtcNow.AddDays(20).Date.AddHours(22),
                    IsDeleted = false
                }
            };

            await context.Events.AddRangeAsync(events);
            await context.SaveChangesAsync();

            // Comments
            var comments = new List<Comment>();

            var firstStory = stories[0];
            var secondStory = stories[1];
            var thirdStory = stories[2];
            var mysteryStory = stories.First(s => s.CategoryId == mystery.Id);
            var thrillerStory = stories.First(s => s.CategoryId == thriller.Id);

            comments.AddRange(new[]
            {
                new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = "This one was seriously creepy. The footprints detail was excellent.",
                    AuthorId = writerUser.Id,
                    StoryId = firstStory.Id,
                    CreatedOn = DateTime.UtcNow.AddDays(-1)
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = "I would definitely read a longer version of this story.",
                    AuthorId = normalUser.Id,
                    StoryId = firstStory.Id,
                    CreatedOn = DateTime.UtcNow.AddHours(-20)
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = "The tension builds really well here.",
                    AuthorId = writerUser.Id,
                    StoryId = secondStory.Id,
                    CreatedOn = DateTime.UtcNow.AddHours(-18)
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = "That ending was unsettling in the best possible way.",
                    AuthorId = normalUser.Id,
                    StoryId = thirdStory.Id,
                    CreatedOn = DateTime.UtcNow.AddHours(-15)
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = "The atmosphere in this mystery story is great.",
                    AuthorId = writerUser.Id,
                    StoryId = mysteryStory.Id,
                    CreatedOn = DateTime.UtcNow.AddHours(-12)
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = "I liked how quickly this one pulled me in.",
                    AuthorId = normalUser.Id,
                    StoryId = thrillerStory.Id,
                    CreatedOn = DateTime.UtcNow.AddHours(-10)
                }
            });

            await context.Comments.AddRangeAsync(comments);
            await context.SaveChangesAsync();
        }
    }
}