using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OwO_Bot.Functions
{
    class Wc : IDisposable
    {
        public WebClient Client { get; set; }

        public Wc()
        {
            Client = new WebClient
            {
                Headers = new WebHeaderCollection
                {
                    {"User-Agent", "Top Yiff Bot/1.0 (by @ClubFlank on Twitter)"}
                }
            };
        }

        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}
