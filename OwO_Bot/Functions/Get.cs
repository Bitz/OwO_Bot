using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using HtmlAgilityPack;
using OwO_Bot.Models;
using RedditSharp;
using RedditSharp.Things;
using static OwO_Bot.Constants;

namespace OwO_Bot.Functions
{
    class Get
    {
        public static Reddit Reddit()
        {
            BotWebAgent webAgent = new BotWebAgent(
                Config.reddit.username,
                Config.reddit.password,
                Config.reddit.client_id,
                Config.reddit.secret_id,
                Config.reddit.callback_url);
            return new Reddit(webAgent, true);
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        public class RedditPost
        {
            public static string GetUrl(Hashing.ImgHash image)
            {
                return $"https://reddit.com/r/{image.SubReddit}/comments/{image.PostId}";
            }

            public static string GenerateTitle(E621Search.SearchResult image, string additionalString = "")
            {
                string artistString = "Unknown";
                if (image.Artist != null)
                {
                    artistString = image.Artist.Count == 0 ? "Unknown" : string.Join(" + ", image.Artist.Select(
                        s => ToTitleCase(s.Replace(" (Artist", ""))));
                }

                Dictionary<string, string> genderLetterPairings =
                    new Dictionary<string, string>
                    {
                        {"ambiguous_gender", "A"},
                        {"male", "M"},
                        {"dickgirl", "D"},
                        {"cuntboy", "C"},
                        {"intersex", "I"},
                        {"female", "F"},
                        {"maleherm", "m"},
                        {"herm", "H"}
                    };
                List<string> genderList = new List<string>();
                List<string> tags = image.Tags.ToLower().Split(' ').Where(x => x.Contains("/") || genderLetterPairings.ContainsKey(x) || x == "solo").ToList();
                
                foreach (var tag in tags)
                {
                    if (tag.Contains("/"))
                    {
                        var parts = tag.Split('/');
                        string thisPairing = string.Empty;
                        if (parts.Any(x => genderLetterPairings.ContainsKey(x)))
                        {
                        foreach (var tagPart in parts)
                        {
                            thisPairing += genderLetterPairings.FirstOrDefault(x => x.Key == tagPart).Value;
                        }
                        genderList.Add(thisPairing);
                        }
                    } else if (tags.Any(x => x == "solo") && tag != "solo")
                    {
                        var soloTag = genderLetterPairings.FirstOrDefault(x => x.Key == tag);
                        genderList.Add(soloTag.Value);
                    }
                }

                string genderGroupings = string.Join(" ", genderList);

                string mainTitle = "Default title";

                string result = $"{mainTitle} [{genderGroupings}] ({artistString}) {additionalString}";


                return result.Trim();
            }
        }

        public class HashPair
        {
            static readonly List<string> GoodExtensions = new List<string> {".jpg", ".png", ".gif", ".jepg"};

            public static Hashing.ImgHash FromPost(Post post)
            {
                Hashing.ImgHash response = new Hashing.ImgHash();
                string path = post.Url.ToString();
                string extension = Path.GetExtension(path);
                string returnUrl = GoodExtensions.Any(x => x.Equals(extension) && !String.IsNullOrEmpty(extension))
                    ? post.Url.ToString()
                    : GetOg(post.Url.ToString());
                Uri test;
                response.IsValid = Uri.TryCreate(returnUrl, UriKind.Absolute, out test) &&
                                   (test.Scheme == Uri.UriSchemeHttp || test.Scheme == Uri.UriSchemeHttps);
                response.Url = returnUrl;
                response.PostId = post.Id;
                response.SubReddit = post.SubredditName;
                if (response.IsValid)
                {
                    response.ImageHash = GetHash(response.Url);
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

            private static string FetchHtml(string url)
            {
                string o = "";

                try
                {
                    HttpWebRequest oReq = (HttpWebRequest) WebRequest.Create(url);
                    oReq.UserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
                    HttpWebResponse resp = (HttpWebResponse) oReq.GetResponse();
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

            public static byte[] GetHash(string url)
            {
                //Download the image...
                List<byte> lByte = new List<byte>();
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    //TODO, support for gif and webms
                    Bitmap bmpSource = new Bitmap(responseStream);
                    //create new image with 16x16 pixel
                    Bitmap bmpMin = new Bitmap(bmpSource, new Size(16, 16));
                    for (int j = 0; j < bmpMin.Height; j++)
                    {
                        for (int i = 0; i < bmpMin.Width; i++)
                        {
                            lByte.Add(bmpMin.GetPixel(i, j).GetBrightness() < 0.5f ? (byte) 0x1 : (byte) 0x0);
                        }
                    }
                }
                return lByte.ToArray();
            }

            public static double CalculateSimilarity(byte[] image1, byte[] image2)
            {
                int equalElements = image1.Zip(image2, (i, j) => i == j).Count(eq => eq);
                double equivalence = (double) equalElements / Math.Max(image1.Length, image2.Length);
                return equivalence;
            }
        }


        public static string ToTitleCase(string s)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(s);
        }
    }
}
