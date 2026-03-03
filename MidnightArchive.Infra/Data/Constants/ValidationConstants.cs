using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Infra.Data.Constants
{
	public class ValidationConstants
	{
		// CATEGORY Validation Constants
		public const int CategoryTitleMaxLength = 150;
		public const int CategoryTitleMinLength = 5;

		public const int CategoryDescriptionMaxLength = 400;
		public const int CategoryDescriptionMinLength = 10;


		// STORY Validation Constants
		public const int StoryTitleMaxLength = 150;
		public const int StoryTitleMinLength = 5;

		public const int StoryContentMaxLength = 50000;
		public const int StoryContentMinLength = 500;

		//COMMENT Validation Constants
		public const int CommentContentMaxLength = 1000;
		public const int CommentContentMinLength = 3;

		// Required Message 
		public const string RequiredErrorMessage = "{0} is required!";
		public const string RequiredLengthMessage = "{0} must be between {2} and {1} symbols!";
	}
}

