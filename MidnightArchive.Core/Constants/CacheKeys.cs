using System;
using System.Collections.Generic;
using System.Text;

namespace MidnightArchive.Core.Constants
{
    public class CacheKeys
    {
        public const string StoriesAll = "stories_all";

        public static string StoriesByCategory(int categoryId) => $"stories_category_{categoryId}";
    }
}
