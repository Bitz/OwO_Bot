
using System;

namespace OwO_Bot.Models
{
    class Misc
    {
        public class PostRequest
        {
            public string RequestUrl { get; set; }
            public long RequestSize { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public long E621Id { get; set; }
            public string ResultUrl { get; set; }
            public string DeleteHash { get; set; }
            public string RedditPostId { get; set; }
            public string Subreddit { get; set; }
            public bool IsNsfw { get; set; }
            public DateTime DatePosted { get; set; }
        }
    }
}
