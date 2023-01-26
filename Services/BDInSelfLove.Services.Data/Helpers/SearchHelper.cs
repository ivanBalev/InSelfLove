namespace BDInSelfLove.Services.Data.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class SearchHelper
    {
        public static string[] GetSearchItems(string searchInput)
        {
            searchInput = searchInput.ToLower();

            // Leave only word characters
            searchInput = Regex.Replace(searchInput, @"[^\w\.@\- ]", string.Empty);

            // Return only meaningful words
            return searchInput.Split().Where(w => !CommonWords().Contains(w)).Select(x => x.Trim()).ToArray();
        }

        public static IList<string> CommonWords()
        {
            // TODO: Find a more adequate list of non-meaningful Bulgarian words
            var bgWordsString = "на, и, в, е, от, за, се, пр, с, да, по, през, са," +
                " като, а, си, не, година, до, че, след, име, това, му, при, най, към, или, има," +
                " които, но, дата, място, той, който, град, н, те, община, във, област, време, години," +
                " част, виж, което, село, която, със, много, описание, други, може, окръг, код, препратки," +
                " един, история, външни, също, този, вид, всички, около, та, между, отбор, още, карта, група," +
                " инфо, страна, селото, само, го";
            var bgWordsList = bgWordsString.Split(", ").ToList();

            var enWordsList = new List<string>
                {
                    "the",
                    "be",
                    "to",
                    "of",
                    "and",
                    "a",
                    "in",
                    "that",
                    "have",
                    "i",
                    "it",
                    "for",
                    "not",
                    "on",
                    "with",
                    "he",
                    "as",
                    "you",
                    "do",
                    "at",
                    "this",
                    "but",
                    "his",
                    "by",
                    "from",
                    "they",
                    "we",
                    "say",
                    "her",
                    "she",
                    "or",
                    "an",
                    "will",
                    "my",
                    "one",
                    "all",
                    "would",
                    "there",
                    "their",
                    "what",
                    "so",
                    "up",
                    "out",
                    "if",
                    "about",
                    "who",
                    "get",
                    "which",
                    "go",
                    "me",
                    "when",
                    "make",
                    "can",
                    "like",
                    "time",
                    "no",
                    "just",
                    "him",
                    "know",
                    "take",
                    "people",
                    "into",
                    "year",
                    "your",
                    "good",
                    "some",
                    "could",
                    "them",
                    "see",
                    "other",
                    "than",
                    "then",
                    "now",
                    "look",
                    "only",
                    "come",
                    "its",
                    "over",
                    "think",
                    "also",
                    "back",
                    "after",
                    "use",
                    "two",
                    "how",
                    "our",
                    "work",
                    "first",
                    "well",
                    "way",
                    "even",
                    "new",
                    "want",
                    "because",
                    "any",
                    "these",
                    "give",
                    "day",
                    "most",
                    "cant",
                    "us",
                };

            bgWordsList.AddRange(enWordsList);
            return bgWordsList;
        }
    }
}
