using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using OwO_Bot.Functions;
using OwO_Bot.Functions.DAL;
using OwO_Bot.Models;
using RedditSharp;
using RedditSharp.Things;
using static System.Console;
using static OwO_Bot.Constants;
using static OwO_Bot.Functions.C;
using static OwO_Bot.Models.E621Search;
using static OwO_Bot.Models.Hashing;

namespace OwO_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Title = "OwO What's This? Loading bulge...";
            Configuration.Load();
            SetOut(new Writer());
            C.WriteLine("Configuration loaded!");
            Title = "OwO Bot " + Constants.Version;

            int argumentIndex = 0;
            if (args.Length > 0)
            {
                if (int.TryParse(args[0], out argumentIndex))
                {
                    C.WriteLine("Found valid argument!");
                }
            }


            if (argumentIndex == -1)
            {
                DatabaseManagement();
            }

            var subConfig = Config.subreddit_configurations[argumentIndex];

            C.WriteLine($"Running for /r/{subConfig.subreddit}!");

            string saveTags = subConfig.tags;
            WebClient client = new WebClient
            {
                Headers = { ["User-Agent"] = $"OwO Bot/{Constants.Version} (by BitzLeon on Reddit)" }
            };
            string result = client.DownloadString($"https://e621.net/post/index.json?tags={saveTags}&limit=50");

            List<SearchResult> searchObject = JsonConvert.DeserializeObject<List<SearchResult>>(result);

            //Hide tags that we were unable to hide earlier because of the 6 tag limit, generally, things that aren't "furry" per se.
            string[] hideTags = subConfig.hide.Split(' ');
            searchObject = searchObject.Where(results => ! hideTags.Any(tagsToHide => results.Tags.Contains(tagsToHide))).ToList();

            if (searchObject.Count == 0)
            {
                C.WriteLine("Searching e621 returned no results.");
                Environment.Exit(1);
            }
            Reddit reddit = Get.Reddit();

            //Login to reddit
            reddit.LogIn(Config.reddit.username, Config.reddit.password);

            if (reddit.User.FullName.ToLower() != Config.reddit.username.ToLower())
            {
                C.WriteLine("Unable to verify login details. Ensure ALL your credentials are correct.");
                Environment.Exit(2);
            }
            else
            {
                C.WriteLine("Logged into Reddit!");
            }
            C.WriteLine("Getting post IDs from database.");
            DbPosts dbConnection = new DbPosts();
            //Get all post Ids from database. We don't want to grb the entire blob yet- those are a bit heavy!
            List<ImgHash> dbPostIds = dbConnection.GetAllIds();

            Subreddit subreddit = reddit.GetSubreddit(subConfig.subreddit);
            C.WriteLine("Getting most recent posts...");
            //Get all the posts from reddit.
            List<Post> newPosts =
                subreddit.New.Where(x => !x.IsSelfPost && x.Created >= DateTimeOffset.Now.AddDays(-1)).ToList();
            //Remove all intersecting items. If we find it in the database, we don't need to recalculate the hash.
            newPosts = newPosts.Where(x => dbPostIds.All(d => x.Id != d.PostId)).ToList();
            C.WriteLine($"Grabbed {newPosts.Count} to compare. Converting to hashes...");
           
            string defaultTitle = Title;
            int progressCounter = 0;
            int totalPosts = newPosts.Count;
            Title = $"{defaultTitle} [{progressCounter}/{totalPosts}]";
            foreach (var newPost in newPosts)
            {
                progressCounter++;
                var timer = new Stopwatch();
                timer.Start();
                Write($"Working on {newPost.Id}...");
                ImgHash thisPair = Get.HashPair.FromPost(newPost);
                if (dbConnection.AddPostToDatabase(thisPair))
                {
                    WriteLineNoTime("Added to database...");
                }
                timer.Stop();
                WriteLineNoTime($"Done in {timer.ElapsedMilliseconds}ms!");
                Title = progressCounter > totalPosts
                    ? $"{defaultTitle} [DONE!]"
                    : $"{defaultTitle} [{progressCounter}/{totalPosts}]";
            }
            Title = defaultTitle;

            List<ImgHash> dbPosts = dbConnection.GetAllValidPosts();

            bool willXPost = false;
            SearchResult imageToPost = null;
            foreach (SearchResult searchResult in searchObject)
            {
                bool isUnique = true;
                byte[] currentImageHash = Get.HashPair.GetHash(searchResult.FileUrl);
                foreach (ImgHash imgHash in dbPosts)
                {
                    double equivalence = Get.HashPair.CalculateSimilarity(currentImageHash, imgHash.ImageHash);
                    if (equivalence > 0.985)
                    {
                        C.WriteLine($"Found equivalency of {equivalence:P1}.");

                        if (String.Equals(subConfig.subreddit, imgHash.SubReddit, StringComparison.OrdinalIgnoreCase))
                        {
                            C.WriteLine("Image was posted on this sub already.");
                            isUnique = false;
                        }
                        else //We found the image posted on another sub. Todo logic for xposting.
                        {
                            C.WriteLine("But the image was uploaded to another sub...");
                            willXPost = true;
                        }
                        break;
                    }
                }
                if (isUnique)
                {
                    imageToPost = searchResult;
                    break;
                }
            }

            if (imageToPost == null)
            {
                C.WriteLine("No image found to post...");
                Environment.Exit(0);
            }

            List<string> pictureExtensions = new List<string>{"jpg", "png", "jpeg"};
            List<string> animationExtensions = new List<string>{"gif", "webm"};

            Misc.PostRequest request = new Misc.PostRequest
            {
                Title = Get.RedditPost.GenerateTitle(imageToPost),
                Description = imageToPost.Description,
                RequestUrl = imageToPost.FileUrl,
                IsNsfw = imageToPost.Rating == "e"
            };


            //Upload to either imgur or gyfcat depending on the type.
            if (pictureExtensions.Contains(imageToPost.FileExt))
            {
                Upload.PostToImgur(ref request);
            }
            else if (animationExtensions.Contains(imageToPost.FileExt))
            {
                Upload.PostToGfycat(ref request);
            }
            Post post = subreddit.SubmitPost(request.Title, request.ResultUrl);
            if (request.IsNsfw)
            {
                post.MarkNSFW();
            }
            request.PostId = post.Id;
            request.DatePosted = DateTime.Now;
        }

        /// <summary>
        /// Utility that will remove all db entries that are older than x days as repopulate the database with fresh data for any new entries across all subs that are configured
        /// </summary>
        private static void DatabaseManagement()
        {
            Reddit reddit = Get.Reddit();
            C.WriteLine("Database management mode entered.");
            DbPosts dbPosts = new DbPosts();
            C.WriteLine($"Deleteing all Posts older than {Config.reddit.Check_Back_X_Days} days...");
            C.WriteLine($"{dbPosts.DeleteAllPostsOlderThan(Config.reddit.Check_Back_X_Days)} Posts deleted!");
            List<Post> postsOnReddit = new List<Post>();
            reddit.LogIn(Config.reddit.username, Config.reddit.password);
            foreach (configurationSub sub in Config.subreddit_configurations)
            {
                C.WriteLine($"Collecting posts for /r/{sub.subreddit}!");
                Subreddit subby = reddit.GetSubreddit(sub.subreddit);
                postsOnReddit.AddRange(subby.New
                    .Where(x => !x.IsSelfPost && x.Created >= DateTimeOffset.Now.AddDays(-Config.reddit.Check_Back_X_Days))
                    .ToList());
            }

            List<ImgHash> postsInDb = dbPosts.GetAllIds();

            //Don't need to process posts that are already hashed in the database!
            postsOnReddit = postsOnReddit.Where(x => postsInDb.All(d => x.Id != d.PostId)).ToList();

            C.WriteLine($"Found {postsOnReddit.Count} Posts to be added to database.");

            string defaultTitle = Title;
            int progressCounter = 0;
            int totalPosts = postsOnReddit.Count;
            Title = $"{defaultTitle} [{progressCounter}/{totalPosts}]";
            foreach (var newPost in postsOnReddit)
            {
                progressCounter++;
                Write($"Working on {newPost.Id}...");
                Stopwatch timer = new Stopwatch();
                timer.Start();
                ImgHash thisPair = Get.HashPair.FromPost(newPost);
                dbPosts.AddPostToDatabase(thisPair);
                timer.Stop();
                WriteLineNoTime($"Done in {timer.ElapsedMilliseconds}ms!");
                Title = progressCounter > totalPosts
                    ? $"{defaultTitle} [DONE!]"
                    : $"{defaultTitle} [{progressCounter}/{totalPosts}]";
            }
            C.WriteLine("Database updated!");
            Environment.Exit(0);
        }


        /// <summary>
        /// Redirect all output to our logger as well as our default console.
        /// </summary>
        class Writer : TextWriter
        {
            readonly StreamWriter _logfile = Logging.CreateNew();
            private readonly TextWriter _originalOut;

            public Writer()
            {
                _originalOut = Out;
            }

            public override void WriteLine(string message)
            {
                _originalOut.WriteLine($"{message}");
                _logfile.WriteLine($"{message}");
            }

            public override void Write(string message)
            {
                _originalOut.Write($"{message}");
                _logfile.Write($"{message}");
            }

            public override Encoding Encoding => new ASCIIEncoding();
        }
    }

    
}
