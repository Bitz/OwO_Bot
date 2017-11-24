using System;

namespace OwO_Bot.Models
{
    class Hashing
    {
        public class ImgHash
        {
            public string Url { get; set; }
            public bool IsValid { get; set; }
            public byte[] ImageHash { get; set; }
            public string SubReddit { get; set; }
            public string PostId { get; set; }
            public DateTime CreatedDate { get; set; }
        }
    }
}
