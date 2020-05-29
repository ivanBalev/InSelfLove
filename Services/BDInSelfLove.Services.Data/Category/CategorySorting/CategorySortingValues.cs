using System.Collections.Generic;

namespace BDInSelfLove.Services.Data.Category.CategorySorting
{
    public static class CategorySortingValues
    {
        public static class TimeCriteria
        {
            public const string AllPosts = "all posts";
            public const string Day = "day";
            public const string Month = "month";
            public const string Year = "year";

            public static List<string> GetAll() => new List<string> { AllPosts, Day, Month, Year };
        }

        public static class GroupingCriteria
        {
            public const string DateCreated = "date created";
            public const string Author = "author";
            public const string Replies = "replies";
            public const string Topic = "topic";

            public static List<string> GetAll() => new List<string> { DateCreated, Author, Replies, Topic };
        }

        public static class OrderingCriteria
        {
            public const string Descending = "descending";
            public const string Ascending = "ascending";

            public static List<string> GetAll() => new List<string> { Descending, Ascending };
        }
    }
}
