
using System;

namespace OwO_Bot.Models
{
    class Misc
    {
        public class PostRequest
        {
            public string RequestUrl { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string ResultUrl { get; set; }
            public string DeleteHash { get; set; }
            public string PostId { get; set; }
            public bool IsNsfw { get; set; }
            public DateTime DatePosted { get; set; }
        }
    }
}
