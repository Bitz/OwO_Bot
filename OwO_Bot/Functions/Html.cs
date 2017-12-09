using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace OwO_Bot.Functions
{
    class Html
    {
        public static string FetchHtml(string url)
        {
            string htmlBody = "";

            try
            {
                HttpWebRequest oReq = (HttpWebRequest)WebRequest.Create(url);
                oReq.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
                HttpWebResponse resp = (HttpWebResponse)oReq.GetResponse();
                Stream stream = resp.GetResponseStream();
                if (stream != null)
                {
                    StreamReader reader = new StreamReader(stream);
                    htmlBody = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return htmlBody;
        }

        public class Title
        {
            public static string SplitBy(string s)
            {
                string returnedTitle = string.Empty;
                var firstOrDefault = s.Split(new[] { " by " }, StringSplitOptions.None)
                    .FirstOrDefault();
                if (firstOrDefault != null)
                {
                    returnedTitle = firstOrDefault.Trim();
                }
                return returnedTitle;
            }

            public static string GetTitle(string url)
            {
                string html = FetchHtml(url);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                HtmlNode firstOrDefault = doc.DocumentNode.Descendants("title").FirstOrDefault();
                return firstOrDefault != null ? WebUtility.HtmlDecode(firstOrDefault.InnerHtml) : string.Empty;
            }
        }

        public static string GetOg(string url)
        {
            //Handle edge cases
            if (url.Contains("mobile.twitter.com"))
            {
                url = url.Replace("mobile.", String.Empty);
            }

            if (url.Contains("tumblr.com/image/"))
            {
                url = url.Replace("/image/", "/post/");
            }

            string resultUrl = "";
            string html = FetchHtml(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection list = doc.DocumentNode.SelectNodes("//meta");
            if (list == null) return string.Empty;
            try
            {
                List<HtmlNode> ogImageNodes = list
                    .Where(x => x.Attributes["property"]?.Value == "og:image" &&
                                x.Attributes["property"].Value != null).ToList();
                //Prefer any format vs gif
                var first = ogImageNodes.First(x => x.Attributes["content"].Value.EndsWith(".jpg")
                                                    || x.Attributes["content"].Value.EndsWith(".jpeg")
                                                    || x.Attributes["content"].Value.EndsWith(".png"));
                resultUrl = first != null
                    ? first.Attributes["content"].Value
                    : ogImageNodes.First().Attributes["content"].Value;
            }
            catch (Exception)
            {
                // ignored
            }

            return resultUrl;
        }
    }
}
