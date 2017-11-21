using System;
using System.Collections.Generic;

namespace OwO_Bot.Models
{
    class Hashing
    {
        public class ImgHash
        {
            public int Id { get; set; }
            public string Url { get; set; }
            public bool IsValid { get; set; }
            public List<bool> Hash { get; set; }
            public string ImageHash { get; set; }
            public string PostId { get; set; }
            public DateTime CreatedDate { get; set; }
        }
    }
}
