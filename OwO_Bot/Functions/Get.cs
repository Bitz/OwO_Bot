using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using OwO_Bot.Models;
using RedditSharp.Things;

namespace OwO_Bot.Functions
{
    class Get
    {
        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        public class HashPair
        {
            static readonly List<string> GoodExtensions = new List<string> { ".jpg", ".png", ".gif", ".jepg" };
            
            public static Hashing.ImgHash FromPost(Post post)
            {
                Hashing.ImgHash response = new Hashing.ImgHash();
                string path = post.Url.ToString();
                string extension = Path.GetExtension(path);
                string returnUrl = GoodExtensions.Any(x => x.Equals(extension) && !String.IsNullOrEmpty(extension)) ? post.Url.ToString() : GetOg(post.Url.ToString());
                Uri test;
                response.IsValid = Uri.TryCreate(returnUrl, UriKind.Absolute, out test) && (test.Scheme == Uri.UriSchemeHttp || test.Scheme == Uri.UriSchemeHttps);
                response.Url = returnUrl;
                response.PostId = post.Id;
                if (response.IsValid)
                {
                    response.Hash = GetHash(response.Url);
                }
                response.CreatedDate = DateTime.Now;
                return response;
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
                    List<HtmlNode> ogImageNodes = list.Where(x => x.Attributes["property"]?.Value == "og:image" && x.Attributes["property"].Value != null).ToList();
                    //Prefer any format vs gif
                    var first = ogImageNodes.First(x => x.Attributes["content"].Value.EndsWith(".jpg")
                                                        || x.Attributes["content"].Value.EndsWith(".jpeg")
                                                        || x.Attributes["content"].Value.EndsWith(".png"));
                    resultUrl = first != null ? first.Attributes["content"].Value : ogImageNodes.First().Attributes["content"].Value;
                }
                catch (Exception)
                {
                    // ignored
                }

                return resultUrl;
            }

            private static string FetchHtml(string url)
            {
                string o = "";

                try
                {
                    HttpWebRequest oReq = (HttpWebRequest)WebRequest.Create(url);
                    oReq.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
                    HttpWebResponse resp = (HttpWebResponse)oReq.GetResponse();
                    Stream stream = resp.GetResponseStream();
                    if (stream != null)
                    {
                        StreamReader reader = new StreamReader(stream);
                        o = reader.ReadToEnd();
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                return o;
            }

            public static List<bool> GetHash(string url)
            {
                //Download the image...
                List<bool> lResult = new List<bool>();
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    Bitmap bmpSource = new Bitmap(responseStream);
                    //create new image with 16x16 pixel
                    Bitmap bmpMin = new Bitmap(bmpSource, new Size(16, 16));
                    for (int j = 0; j < bmpMin.Height; j++)
                    {
                        for (int i = 0; i < bmpMin.Width; i++)
                        {
                            //reduce colors to true / false                
                            lResult.Add(bmpMin.GetPixel(i, j).GetBrightness() < 0.5f);
                        }
                    }
                }
                return lResult;
            }


        }
    }
}
