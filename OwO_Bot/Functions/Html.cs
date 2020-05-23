using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using static System.Reflection.Assembly;
using static OwO_Bot.Constants;
using static OwO_Bot.Functions.Get;

namespace OwO_Bot.Functions
{
    class Html
    {
        public static Image GetImageFromUrl(string url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            Image image;
            try
            {
                image = Image.FromStream(responseStream);
            }
            catch (Exception)
            {
                //Fallback to getting image from this
                image = GetVideoFirstFrameFromUrl(url);
            }
            return image;
        }

        public static byte[] GetArrayFromUrl(string url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            return Convert.StreamToByte(responseStream); ;
        }

        public static string FindThumbnail(string url)
        {
            string thumbUrl = string.Empty;
            List<string> imageExtensions = new List<string> { ".png", ".jpg", ".jpeg", ".gifv" };
            string ext = Path.GetExtension(url);
            if (url.Contains("imgur.com") && imageExtensions.Contains(ext))
            {
                string thumbrl = url.Replace(ext, "");
                thumbUrl = $"{thumbrl}m{ext}";
            }
            else if (url.Contains("gfycat.com"))
            {
                Uri uri = new Uri(url);
                thumbUrl = $"{uri.Scheme}://thumbs.{url.Replace(uri.Scheme + "://", "")}-mobile.jpg";
            }
            return thumbUrl;
        }

        public static Image GetVideoFirstFrameFromUrl(string video)
        {
            C.WriteNoTime("Converting image with ffmpeg...");
            string location = GetExecutingAssembly().Location;
            string absoluteCurrentDirectory = Path.GetDirectoryName(location);
            string thumbLocation = Path.Combine(absoluteCurrentDirectory, "temp.png");
            string ffmpegLocation = Path.Combine(absoluteCurrentDirectory, "ffmpeg.exe");
            var cmd = $"-loglevel quiet -y -i \"{video}\" -vframes 1 -s {PixelSize}x{PixelSize} \"{thumbLocation}\"";
            var calculatedffmpegPath = IsRunningOnMono() ? "/usr/bin/ffmpeg" : ffmpegLocation;
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = calculatedffmpegPath,
                Arguments = cmd
            };
            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            while (!process.HasExited)
            {
                process.WaitForExit(100);
            }
            C.WriteLineNoTime("Done!");
            return Image.FromFile(thumbLocation);
        }

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
                    .Where(x => x.Attributes["property"]?.Value == "og:image" || x.Attributes["name"]?.Value == "twitter:image").ToList();
                //Prefer any format vs gif
                var first = ogImageNodes.First(x => 
                    x.Attributes["content"].Value.EndsWith(".jpg")
                    || x.Attributes["content"].Value.EndsWith(".jpg?play") //Gifv
                    || x.Attributes["content"].Value.EndsWith(".jpeg")
                    || x.Attributes["content"].Value.EndsWith(".png")
                    );
                resultUrl = first != null
                    ? first.Attributes["content"].Value.TrimEnd("?play")
                    : ogImageNodes.First().Attributes["content"].Value.TrimEnd("?play");
            }
            catch (Exception)
            {
                // ignored
            }

            return resultUrl;
        }
    }
}
