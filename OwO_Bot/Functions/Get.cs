using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using OwO_Bot.Models;
using RedditSharp;
using static System.Reflection.Assembly;
using static OwO_Bot.Constants;

namespace OwO_Bot.Functions
{
    class Get
    {
        public static Image Thumbnail(string video)
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
            public static string GetUrl(Models.Hashing.ImgHash image)
            {
                return $"https://reddit.com/r/{image.SubReddit}/comments/{image.PostId}";
            }

            public static string GenerateTitle(E621Search.SearchResult image, string additionalString = "")
            {
                string artistString = "Unknown Artist";
                image.Artist.Remove("conditional_dnp");
                if (image.Artist != null)
                {
                    artistString = image.Artist.Count == 0 ? "Unknown" : string.Join(" + ", image.Artist.Select(
                        s => ToTitleCase(s.Replace("_(artist)", ""))));
                }

                Dictionary<string, string> genderLetterPairings =
                    new Dictionary<string, string>
                    {
                       //{"ambiguous_gender", "A"},
                        {"male", "M"},
                        {"dickgirl", "D"},
                        {"cuntboy", "C"},
                       // {"intersex", "I"},
                        {"female", "F"},
                        {"maleherm", "H"},
                        {"herm", "H"},
                        {"tentacles", "T"}
                    };
                List<string> genderList = new List<string>();
                List<string> tags = image.Tags.ToLower().Split(' ').Where(x => x.Contains("/") || genderLetterPairings.ContainsKey(x)).ToList();

                foreach (var tag in tags)
                {
                    if (tag.Contains('/')) //Do pairings
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
                    }
                    else if (!tags.Any(x => x.Contains('/'))) //If there were no pairings, this image only has solos
                    {
                        var soloTag = genderLetterPairings.FirstOrDefault(x => x.Key == tag);
                        genderList.Add(soloTag.Value);
                    }
                }

                string genderGroupings;

                if (genderList.Count == 2)
                {
                    genderGroupings = string.Join("", genderList);
                }
                else
                {
                    genderGroupings = genderList.Count == 0 ? "MF" : string.Join(" ", genderList);
                }


                string mainTitle = MainTitle(image);

                string result = $"{mainTitle} [{genderGroupings}] ({artistString}) {additionalString}";

                return result.Trim();
            }
        }

        private static string MainTitle(E621Search.SearchResult image)
        {
            string returnedTitle = string.Empty;
            //Some titles can be gotten automatically from their appropriate sources. Other times, we will have to send an email asking for a nice title.
            if (image.Sources != null && image.Sources.Count > 0)
            {
                foreach (var imageSource in image.Sources)
                {
                    try //Lots can go wrong here, but it should not break anything. If anything here fails, continue on asking the user for a title.
                    {
                        if (imageSource.Contains("furaffinity.net") && imageSource.Contains("/view/"))
                        {
                            returnedTitle = Html.Title.GetTitle(imageSource);
                            returnedTitle = Html.Title.SplitBy(returnedTitle);
                            break;
                        }
                        if (imageSource.Contains("inkbunny.net") && imageSource.Contains("/s/"))
                        {
                            returnedTitle = Html.Title.GetTitle(imageSource);
                            returnedTitle = Html.Title.SplitBy(returnedTitle);
                            break;
                        }
                        if (imageSource.Contains("deviantart.com") && imageSource.Contains("/art/"))
                        {
                            returnedTitle = Html.Title.GetTitle(imageSource);
                            returnedTitle = Html.Title.SplitBy(returnedTitle);
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        C.WriteLine(e.ToString());
                    }
                    
                }
            }
            //If we find any of these things in the title, we are still going to send the email because we consider the title "bad"
            List<string> unapprovedTitleElements = new List<string> {
                "commission" ,
                "comm",
                "ych",
                "character",
                "page", "pg",
                "/", "\\",
                "(", ")",
                "[", "]",
                ":", ";",
                "|", "#",
                "&", "<",
                ">"
            };

            if (!string.IsNullOrEmpty(returnedTitle) && ! unapprovedTitleElements.Any(x => returnedTitle.ToLower().Contains(x)))
            {
                return returnedTitle;
            }

            #region Title Mailer
            Mail mailer = new Mail();
            string mailBody = string.Empty;
            if (!string.IsNullOrEmpty(returnedTitle))
            {
                mailBody += $"TITLE: {returnedTitle}\r\n\r\n";
            }
           
            mailBody += $"{image.FileUrl} (" + Convert.BytesToReadableString(image.FileSize) + ")\r\n\r\n";
            List<string> tags = image.Tags.ToLower().Split(' ').ToList();

            mailBody += "TAGS:\r\n\r\n";
            foreach (var tag in tags)
            {
                mailBody += tag + "\r\n";
            }

            if (image.Sources != null && image.Sources.Count > 0)
            {
                mailBody += "\r\n\r\nSOURCE(S):\r\n\r\n";
                foreach (var source in image.Sources)
                {
                    mailBody += source + "\r\n";
                }
            }
            string emailTitle = $"Title {image.Id}";
            mailer.Send(emailTitle, mailBody);
            returnedTitle = mailer.Recieve($"Title {image.Id}", image.Id).Trim();
            #endregion
            return returnedTitle;
        }

        public static string ToTitleCase(string s)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(s);
        }

       



        


        
    }
}
