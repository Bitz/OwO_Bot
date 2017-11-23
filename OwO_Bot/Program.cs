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
            BotWebAgent webAgent = new BotWebAgent(
                Config.reddit.username,
                Config.reddit.password,
                Config.reddit.client_id,
                Config.reddit.secret_id,
                Config.reddit.callback_url);
            Reddit reddit = new Reddit(webAgent, true);

            if (argumentIndex == -1)
            {
                C.WriteLine("Database management mode entered.");
                DbPosts dbPosts = new DbPosts();
                C.WriteLine("Deleteing all Posts older than 30 days...");
                C.WriteLine($"{dbPosts.DeleteAllPostsOlderThan()} Posts deleted!");
                List<Post> postsOnReddit = new List<Post>();
                reddit.LogIn(Config.reddit.username, Config.reddit.password);
                foreach (configurationSub sub in Config.subreddit_configurations)
                {
                    C.WriteLine($"Collecting posts for /r/{sub.subreddit}!");
                    Subreddit subby = reddit.GetSubreddit(sub.subreddit);
                    postsOnReddit.AddRange(subby.New.Where(x => !x.IsSelfPost && x.Created >= DateTimeOffset.Now.AddDays(-1)).ToList());
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
                    WriteNoTime($"Working on {newPost.Id}...");
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    ImgHash thisPair = Get.HashPair.FromPost(newPost);
                    if (thisPair.IsValid)
                    {
                        dbPosts.AddPostToDatabase(thisPair);
                    }
                    timer.Stop();
                    WriteNoTime($"Done in {timer.ElapsedMilliseconds}ms!");
                    Title = progressCounter > totalPosts ? $"{defaultTitle} [DONE!]" : $"{defaultTitle} [{progressCounter}/{totalPosts}]";
                }
                C.WriteLine("Database updated!");
                Environment.Exit(0);
            }

            var subConfig = Config.subreddit_configurations[argumentIndex];

            C.WriteLine($"Running for /r/{subConfig.subreddit}!");

            string saveTags = subConfig.tags;
            WebClient client = new WebClient
            {
                Headers = { ["User-Agent"] = $"OwO Bot/{Constants.Version} (by BitzLeon on Reddit)" }
            };
            string result = client.DownloadString($"https://e621.net/post/index.json?tags={saveTags}&limit=50");

            List<E621Search.SearchResult> searchObject = JsonConvert.DeserializeObject<List<E621Search.SearchResult>>(result);

            //Hide tags that we were unable to hide earlier because of the 6 tag limit, generally, things that aren't "furry" per se.
            string[] hideTags = subConfig.hide.Split(' ');
            searchObject = searchObject.Where(results => ! hideTags.Any(tagsToHide => results.Tags.Contains(tagsToHide))).ToList();

            if (searchObject.Count == 0)
            {
                C.WriteLine("Searching e621 returned no results.");
                Environment.Exit(1);
            }

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

            Subreddit subreddit = reddit.GetSubreddit(subConfig.subreddit);
            C.WriteLine("Getting most recent posts...");
            List<Post> newPosts =
                subreddit.New.Where(x => !x.IsSelfPost && x.Created >= DateTimeOffset.Now.AddDays(-1)).ToList();
            C.WriteLine($"Grabbed {newPosts.Count} to compare. Converting to bit arrays... (This may take a while)");
            List<ImgHash> hashPairs = new List<ImgHash>();

            byte[] currentImageHash = Get.HashPair.GetHash(searchObject.First().FileUrl);
            Stopwatch s= new Stopwatch();
            s.Start();
            foreach (var newPost in newPosts)
            {
                ImgHash thisPair = Get.HashPair.FromPost(newPost);
                hashPairs.Add(thisPair);
                if (thisPair.IsValid)
                {
                    //TODO new method
                    var equivalence =  Get.HashPair.CalculateSimilarity(currentImageHash, thisPair.ImageHash);
                    C.WriteLine(equivalence);
                }
            }
            s.Stop();

            C.WriteLine($"We got {hashPairs.Count(x => x.IsValid)} hashes calculated in {s.ElapsedMilliseconds}ms.");

            
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
                _originalOut.WriteLine($"{message}");
                _logfile.WriteLine($"{message}");
            }

            public override Encoding Encoding => new ASCIIEncoding();
        }
    }

    
}
