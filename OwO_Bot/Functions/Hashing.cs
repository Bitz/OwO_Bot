using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using RedditSharp.Things;

namespace OwO_Bot.Functions
{
    class Hashing
    {
        static readonly List<string> GoodExtensions = new List<string> { ".jpg", ".png", ".gif", ".jepg" };

        public static Models.Hashing.ImgHash FromPost(Post post)
        {
            Models.Hashing.ImgHash response = new Models.Hashing.ImgHash();
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
            string html = Get.FetchHtml(url);
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



        public static byte[] GetHash(string url)
        {
            //Download the image...
            List<byte> lByte = new List<byte>();
            Image image;
            if (Path.GetExtension(url) == ".gif" || Path.GetExtension(url) == ".webm")
            {
                image = Get.Thumbnail(url);
            }
            else
            {
                //11% faster to do it this way, so when possible, use this method to generate thumbs instead.
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                image = Image.FromStream(responseStream);
            }
            Bitmap bmpMin = new Bitmap(image, new Size(Constants.PixelSize, Constants.PixelSize));
            for (int j = 0; j < bmpMin.Height; j++)
            {
                for (int i = 0; i < bmpMin.Width; i++)
                {
                    lByte.Add(bmpMin.GetPixel(i, j).GetBrightness() < 0.5f ? (byte)0x1 : (byte)0x0);
                }
            }

            return lByte.ToArray();
        }

        public static double CalculateSimilarity(byte[] image1, byte[] image2)
        {
            int equalElements = image1.Zip(image2, (i, j) => i == j).Count(eq => eq);
            double equivalence = (double)equalElements / Math.Max(image1.Length, image2.Length);
            return equivalence;
        }
    }
}
