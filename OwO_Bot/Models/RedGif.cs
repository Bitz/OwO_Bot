namespace OwO_Bot.Models
{
    class RedGif
    {
        public class Response
        {
            public class Rootobject
            {
                public bool isOk { get; set; }
                public string gfyname { get; set; }
                public string secret { get; set; }
                public string uploadType { get; set; }
                public string fetchUrl { get; set; }
            }

            public class Token
            {
                public string token_type { get; set; }
                public string scope { get; set; }
                public int expires_in { get; set; }
                public string access_token { get; set; }
            }
        }
    }
}