using System;

namespace OwO_Bot.Models
{
    public class Blacklist
    {
        public long PostId { get; set; }
        public string Subreddit { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
