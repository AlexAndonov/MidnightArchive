using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.AccessControl;
using Microsoft.EntityFrameworkCore;
using MidnightArchive.Data.Data.Models;
using MidnightArchive.Data.Models;
using static MidnightArchive.Data.Data.Constants.ValidationConstants;

namespace MidnightArchive.Data.Data.Models;

[Comment("Story class")]
public class Story
{
    [Comment("Story identifier")]
    public Guid Id { get; set; }


    [Required]
    [MaxLength(StoryTitleMaxLength)]
    [Comment("Title of the story")]
    public string Title { get; set; } = null!;


    [Required]
    [MaxLength(StoryContentMaxLength)]
    [Comment("Text content of the story")]
    public string Content { get; set; } = null!;


    [Comment("Date of creation of the story")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [Comment("Indicates the date when story was edited")]
	public DateTime? ModifiedOn { get; set; }


	[Required]
    [Comment("Unique identifier of the story's author")]
    public string AuthorId { get; set; } = null!;

    [ForeignKey(nameof(AuthorId))]
    [Comment("Navigation property for the author")]
    public ApplicationUser Author { get; set; } = null!;


    [Comment("Unique identifier of the story's category")]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    [Comment("Navigation property for the category")]
    public Category Category { get; set; } = null!;

    [Comment("Number of views of the story")]
    public int ViewsCount { get; set; }

    [Comment("Number of likes of the story")]
    public int LikesCount { get; set; }

    [Comment("Indicates whether the author is anonymous - author's choice")]
    public bool IsAnonymous { get; set; }

    [Comment("Indicates whether the story is soft-deleted")]
    public bool IsDeleted { get; set; }

    [Comment("Story's comments")]
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}