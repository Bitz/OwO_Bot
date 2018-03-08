using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ColorThiefDotNet;
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
            Args = args;
            int argumentIndexSub = 0;
            if (args.Length > 0)
            {
                if (int.TryParse(args[0], out argumentIndexSub))
                {
                    C.WriteLine("Found valid argument for subreddit!");
                }
            }

            int argumentIndexMail = 0;
            if (args.Length > 1)
            {
                if (int.TryParse(args[1], out argumentIndexMail))
                {
                    C.WriteLine("Found valid argument for Email!");
                    EmailRecipient = Config.mail.reciever[argumentIndexMail];
                }
            }
            else
            {
                C.WriteLine("Using default mailer!");
                EmailRecipient = Config.mail.reciever[argumentIndexMail];
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

            #region Temporary method for populating colorschemes
            //using (DbPosts p = new DbPosts())
            //{
            //    var noColorScheme = p.GetWithNoColorScheme();

            //    var colorThiefc = new ColorThief();
            //    foreach (var r in noColorScheme)
            //    {
            //        string thumbUrlc = Html.FindThumbnail(r.ResultUrl);
            //        Bitmap ac = new Bitmap(Html.GetImageFromUrl(thumbUrlc));
            //        var color = colorThiefc.GetColor(ac);
            //        p.UpdateColorScheme(r.Id, color.Color.ToHexString());
            //    }
            //}
            #endregion

            if (argumentIndexSub == -1)
            {
                DatabaseManagement();
            }

            var subConfig = Config.subreddit_configurations[argumentIndexSub];
            WorkingSub = subConfig.subreddit;
            string requestSite = subConfig.issafe ? "e926" : "e621";
            C.WriteLine($"Running for /r/{subConfig.subreddit}!");

            string saveTags = $"{subConfig.tags} date:>={DateTime.Now.AddDays(-1):yyyy-MM-dd}";
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
                    result = client.DownloadString($"https://{requestSite}.net/post/index.json?tags={saveTags}&limit=50&page=" + page);
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
                //Filetype filtering
                searchObject = searchObject.Where(results => results.FileExt != "swf").ToList();
                //Hard filtering
                searchObject = searchObject.Where(results => !TagsToHide.Any(tagsToHide => results.Tags.Contains(tagsToHide))).ToList();
                //Soft filtering
                searchObject = searchObject.Where(results => !hideTags.Any(tagsToHide => results.Tags.Contains(tagsToHide))).ToList();
                //Blacklist filtering
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
            Misc.PostRequest request = new Misc.PostRequest
            {
                Description = imageToPost.Description,
                RequestUrl = imageToPost.FileUrl,
                RequestSize = imageToPost.FileSize,
                IsNsfw = imageToPost.Rating == "e",
                E621Id = imageToPost.Id,
                Subreddit = WorkingSub
            };
            
            //Properly recycle data if we are going to be reposting something from another sub. 
            DbPosts dboPosts = new DbPosts();
            var uploadedCheck = dboPosts.GetPostData(imageToPost.Id);
            if (uploadedCheck.Count == 0)
            {
                C.Write("Building title...");
                request.Title = GenerateTitle(imageToPost);
                C.WriteLineNoTime("Done!");
                UploadImage(imageToPost, ref request);
            }
            else
            {
                var first = uploadedCheck.First();
                request.Title = first.Title;
                request.ResultUrl = first.ResultUrl;
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
            
            string parsede621Source =  $"[{requestSite} Source](https://{requestSite}.net/post/show/{imageToPost.Id})";

            string creditsFooter;
            if (MailBasedTitle)
            {
                creditsFooter = !string.IsNullOrEmpty(EmailRecipient.username) ? 
                    $"/u/{EmailRecipient.username}" : 
                    "a helpful user";
            }
            else
            {
                creditsFooter = "Artist";
            }
            string comment = $"{parsedSource} | {parsede621Source} " +
                             "\r\n" +
                             "\r\n" +
                             "---" +
                             "\r\n" +
                             "\r\n" +
                             $"Title by {creditsFooter} | This is a bot | [Info](https://owo.bitz.rocks/) | [Donate](https://owo.bitz.rocks/Donate) | [Report problems](/message/compose/?to=BitzLeon&subject={Config.reddit.username} running OwO Bot {Constants.Version}) | [Source code](https://github.com/Bitz/OwO_Bot)";

            post.Comment(comment);
            request.RedditPostId = post.Id;

            Blacklist imageData = new Blacklist
            {
                PostId = imageToPost.Id,
                CreatedDate = DateTime.Now,
                Subreddit = WorkingSub
            };

            var colorThief = new ColorThief();
            string thumbUrl = Html.FindThumbnail(request.ResultUrl);
            Bitmap a = new Bitmap(Html.GetImageFromUrl(thumbUrl));
            request.ColorScheme = colorThief.GetColor(a).Color.ToHexString();

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

        private static void UploadImage(SearchResult imageToPost, ref Misc.PostRequest request)
        {
            List<string> pictureExtensions = new List<string> { "jpg", "png", "jpeg" };
            List<string> animationExtensions = new List<string> { "gif", "webm" };
            //Upload to either imgur or gyfcat depending on the type.
            if (pictureExtensions.Contains(imageToPost.FileExt))
            {
                Upload.PostToImgur(ref request);
            }
            else if (animationExtensions.Contains(imageToPost.FileExt))
            {
                Upload.PostToGfycat(ref request);
            }
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
