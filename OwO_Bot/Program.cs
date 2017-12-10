using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using RedditSharp;
using RedditSharp.Things;
using static System.Console;
using OwO_Bot.Functions;
using OwO_Bot.Functions.DAL;
using OwO_Bot.Models;
using static OwO_Bot.Constants;
using static OwO_Bot.Functions.Get.RedditPost;
using static OwO_Bot.Models.E621Search;
using static OwO_Bot.Models.Hashing;
using C = OwO_Bot.Functions.C;
using F = OwO_Bot.Functions;

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

            #region Temporary method for populating database with titles.
            //using (DbPosts p = new DbPosts())
            //{
            //    var allPosts = p.GetAllPosts();

                
            //    foreach (Misc.PostRequest s in allPosts)
            //    {
            //        if (string.IsNullOrEmpty(s.Title))
            //        {
            //            var redd = Get.Reddit();
            //            Post sss = (Post)redd.GetThingByFullname($"t3_{s.RedditPostId}");
            //            p.SetTitle(s.E621Id, sss.Title);
            //        }
            //    }
            //}
            #endregion

            if (argumentIndex == -1)
            {
                DatabaseManagement();
            }

            var subConfig = Config.subreddit_configurations[argumentIndex];
            WorkingSub = subConfig.subreddit;

            C.WriteLine($"Running for /r/{subConfig.subreddit}!");

            string saveTags = $"{subConfig.tags} date:{DateTime.Now.AddDays(-1):yyyy-MM-dd}";
            WebClient client = new WebClient
            {
                Headers = { ["User-Agent"] = $"OwO Bot/{Constants.Version} (by BitzLeon on Reddit)" }
            };

            int page = 1;
            List<SearchResult> searchObject = new List<SearchResult>();
            List<Blacklist> blacklist;
            using (DbBlackList dbBlackList = new DbBlackList())
            {
                blacklist = dbBlackList.GetAllIds(WorkingSub);
            }
            
            while (searchObject.Count == 0)
            {
                string result = string.Empty;
                try
                {
                    result = client.DownloadString($"https://e621.net/post/index.json?tags={saveTags}&limit=50&page=" + page);

                }
                catch (WebException)
                {
                    C.WriteLine("No search results found.");
                    Environment.Exit(2);
                }
                var temp = JsonConvert.DeserializeObject<List<SearchResult>>(result);
                if (temp != null)
                {
                    searchObject.AddRange(temp);
                    searchObject = searchObject.Distinct().ToList();
                }
                //Hide tags that we were unable to hide earlier because of the 6 tag limit, generally, things that aren't "furry" per se.
                string[] hideTags = subConfig.hide.Split(' ').Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();
                searchObject = searchObject.Where(results => !hideTags.Any(tagsToHide => results.Tags.Contains(tagsToHide))).ToList();
                searchObject = searchObject.Where(r => !blacklist.Select(x => x.PostId).Contains(r.Id)).ToList();
                page++;
            }
            Reddit reddit = Get.Reddit();

            //Login to reddit
            reddit.LogIn(Config.reddit.username, Config.reddit.password);

            if (reddit.User.FullName.ToLower() != Config.reddit.username.ToLower())
            {
                C.WriteLine("Unable to verify Reddit login details. Ensure ALL your credentials are correct.");
                Environment.Exit(2);
            }

            Subreddit subreddit = reddit.GetSubreddit(subConfig.subreddit);
            C.WriteLine("Getting most recent posts..."); //2 days back should be fine
            //Get all the posts from reddit.
            var newPosts = subreddit.New.Where(x => !x.IsSelfPost && x.Created >= DateTimeOffset.Now.AddDays(-2)).ToList();
            DbSubredditPosts dbSubredditConnection = new DbSubredditPosts();

            //Clean up old posts. No reason to keep them in here.
            C.WriteLine($"Deleteing all Posts older than {Config.reddit.Check_Back_X_Days} days...");
            C.WriteLine($"{dbSubredditConnection.DeleteAllPostsOlderThan(Config.reddit.Check_Back_X_Days)} Posts deleted!");

            //Get all post Ids from database. We don't want to grb the entire blob yet- those are a bit heavy!
            List<ImgHash> dbPostIds = dbSubredditConnection.GetAllIds();
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
                ImgHash thisPair = F.Hashing.FromPost(newPost);
                if (dbSubredditConnection.AddPostToDatabase(thisPair))
                {
                    C.WriteLineNoTime("Added to database...");
                }
                timer.Stop();
                C.WriteLineNoTime($"Done in {timer.ElapsedMilliseconds}ms!");
                Title = progressCounter > totalPosts
                    ? $"{defaultTitle} [DONE!]"
                    : $"{defaultTitle} [{progressCounter}/{totalPosts}]";
            }
            Title = defaultTitle;

            List<ImgHash> dbPosts = dbSubredditConnection.GetAllValidPosts();
            dbSubredditConnection.Dispose(); //Close  the connection. Don't need to keep it open anymore. 
            dbPosts = dbPosts.Where(x => x.SubReddit.ToLower() == subConfig.subreddit.ToLower()).ToList();
            SearchResult imageToPost = null;
            foreach (SearchResult searchResult in searchObject)
            {
                bool isUnique = true;
                byte[] currentImageHash = F.Hashing.GetHash(searchResult.FileUrl);
                
                foreach (ImgHash imgHash in dbPosts)
                {
                    double equivalence = F.Hashing.CalculateSimilarity(currentImageHash, imgHash.ImageHash);
                    if (equivalence > 0.985)
                    {
                        if (String.Equals(subConfig.subreddit, imgHash.SubReddit, StringComparison.OrdinalIgnoreCase))
                        {
                            C.WriteLine($"Found equivalency of {equivalence:P1}.");
                            C.WriteLine("Image was posted on this sub already.");
                            isUnique = false;
                            break;
                        }
                        else //We found the image posted on another sub. logic for xposting goes here... if we actually want it.
                        {
                            //C.WriteLine("But the image was uploaded to another sub...");
                            //willXPost = true;
                        }
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
            else
            {
                C.WriteLine("Found an image to post! Lets start doing things...");
            }

            List<string> pictureExtensions = new List<string>{"jpg", "png", "jpeg"};
            List<string> animationExtensions = new List<string>{"gif", "webm"};

            C.Write("Building title...");
            Misc.PostRequest request = new Misc.PostRequest
            {
                Title = GenerateTitle(imageToPost),
                Description = imageToPost.Description,
                RequestUrl = imageToPost.FileUrl,
                IsNsfw = imageToPost.Rating == "e",
                E621Id = imageToPost.Id,
                Subreddit = WorkingSub
            };
            C.WriteLineNoTime("Done!");

            //Upload to either imgur or gyfcat depending on the type.
            if (pictureExtensions.Contains(imageToPost.FileExt))
            {
                Upload.PostToImgur(ref request);
            }
            else if (animationExtensions.Contains(imageToPost.FileExt))
            {
                Upload.PostToGfycat(ref request);
            }
            C.Write("Posting to Reddit...");
            Post post = subreddit.SubmitPost(request.Title, request.ResultUrl);
            if (request.IsNsfw)
            {
                post.MarkNSFW();
            }
            C.WriteLineNoTime("Done!");
            request.DatePosted = DateTime.Now;
            C.Write("Commenting on Post...");
            string parsedSource;
            if (imageToPost.Sources != null && imageToPost.Sources.Count > 0)
            {
                parsedSource = $"[Original Source]({imageToPost.Sources.FirstOrDefault()})";
            }
            else
            {
                parsedSource = "No source provided";
            }
            string parsede621Source =  $"[e621 Source](https://e621.net/post/show/{imageToPost.Id})";
            string comment = $"[](/sweetiecardbot) {parsedSource} | {parsede621Source} " +
                             "\r\n  \r\n" +
                             "---" +
                             "\r\n  \r\n" +
                             $"This is a bot | [Info](https://bitz.rocks/owo_bot) | [Report problems](/message/compose/?to=BitzLeon&subject={Config.reddit.username} running OwO Bot {Constants.Version}) | [Source code](https://github.com/Bitz/OwO_Bot)";

            post.Comment(comment);
            request.RedditPostId = post.Id;

            Blacklist imageData = new Blacklist
            {
                PostId = imageToPost.Id,
                CreatedDate = DateTime.Now,
                Subreddit = WorkingSub
            };

            //instate a reusable connection rather than a 1 off object.
            using (DbConnector dbConnector = new DbConnector())
            {
                //Saved to prevent rechecking.
                DbBlackList blacklistdb = new DbBlackList(dbConnector);
                blacklistdb.AddToBlacklist(imageData);

                //Saved for later use maybe.
                DbPosts dbPostsFinalSave = new DbPosts(dbConnector);
                dbPostsFinalSave.AddPostToDatabase(request);
            }

            C.WriteLineNoTime("Done!");
        }

        /// <summary>
        /// Utility that will remove all db entries that are older than x days as repopulate the database with fresh data for any new entries across all subs that are configured
        /// </summary>
        private static void DatabaseManagement()
        {
            Reddit reddit = Get.Reddit();
            C.WriteLine("Database management mode entered.");
            DbSubredditPosts dbSubredditPosts = new DbSubredditPosts();
            C.WriteLine($"Deleteing all Posts older than {Config.reddit.Check_Back_X_Days} days...");
            C.WriteLine($"{dbSubredditPosts.DeleteAllPostsOlderThan(Config.reddit.Check_Back_X_Days)} Posts deleted!");
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

            List<ImgHash> postsInDb = dbSubredditPosts.GetAllIds();

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
                C.Write($"Working on {newPost.Id}...");
                Stopwatch timer = new Stopwatch();
                timer.Start();
                ImgHash thisPair = F.Hashing.FromPost(newPost);
                dbSubredditPosts.AddPostToDatabase(thisPair);
                timer.Stop();
                C.WriteLineNoTime($"Done in {timer.ElapsedMilliseconds}ms!");
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
