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

		//Event Validation Constants
		public const int EventTitleMaxLenth = 100;
		public const int EventTitleMinLength = 3;

		public const int EventDescriptionMaxLength = 1000;
		public const int EventDescriptionMinLength = 10;

		// Required Message 
		public const string FieldRequiredErrorMessage = "{0} is required!";
		public const string RequiredLengthMessage = "{0} must be between {2} and {1} symbols!";
	}
}

