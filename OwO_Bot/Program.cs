using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
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
using static System.DateTime;
using static OwO_Bot.Constants;

namespace OwO_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Title = "OwO What's This? Loading bulge...";
            Configuration.Load();
            SetOut(new Writer());
            WriteLine("Configuration loaded!");
            Title = "OwO Bot " + Constants.Version;

            int argumentIndex = 0;
            if (args.Length > 0)
            {
                if (int.TryParse(args[0], out argumentIndex))
                {
                    WriteLine("Found valid argument!");
                }
            }

            if (argumentIndex == -1)
            {
                WriteLine("Database management mode entered.");

                DbConnector con = new DbConnector();
                string ss = "SELECT * FROM owo_bot.posts;";
                var p = new List<SqlParameter>();
                var r = con.ExecuteDataReader(ss, ref p);
                //TODO
                while (r.Read())
                {
                    Console.WriteLine($"{r.GetString(0)} {r.GetString(1)} ");
                }
                Environment.Exit(0);
            }

            var subConfig = Config.subreddit_configurations[argumentIndex];

            WriteLine($"Running for /r/{subConfig.subreddit}!");

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
                WriteLine("Searching e621 returned no results.");
                Environment.Exit(1);
            }

            BotWebAgent webAgent = new BotWebAgent(
                Config.reddit.username,
                Config.reddit.password,
                Config.reddit.client_id,
                Config.reddit.secret_id,
                Config.reddit.callback_url);

            //Create reddit client instance
            Reddit reddit = new Reddit(webAgent, true);
            //Login to reddit
            reddit.LogIn(Config.reddit.username, Config.reddit.password);

            if (reddit.User.FullName.ToLower() != Config.reddit.username.ToLower())
            {
                WriteLine("Unable to verify login details. Ensure ALL your credentials are correct.");
                Environment.Exit(2);
            }
            else
            {
                WriteLine("Logged into Reddit!");
            }

            Subreddit subreddit = reddit.GetSubreddit(subConfig.subreddit);
            WriteLine("Getting most recent posts...");
            List<Post> newPosts =
                subreddit.New.Where(x => !x.IsSelfPost && x.Created >= DateTimeOffset.Now.AddDays(-1)).ToList();
            WriteLine($"Grabbed {newPosts.Count} to compare. Converting to bit arrays... (This may take a while)");
            List<Hashing.ImgHash> HashPairs = new List<Hashing.ImgHash>();

            List<bool> currentImageHash = Get.HashPair.GetHash(searchObject.First().FileUrl);
            Stopwatch s= new Stopwatch();
            s.Start();
            foreach (var newPost in newPosts)
            {
                var thisPair = Get.HashPair.FromPost(newPost);
                HashPairs.Add(thisPair);
                if (thisPair.IsValid)
                {
                    double equalElements = currentImageHash.Zip(thisPair.Hash, (i, j) => i == j).Count(eq => eq) / 256.00;
                    WriteLine(equalElements);
                }
            }
            s.Stop();

            WriteLine($"We got {HashPairs.Count(x => x.IsValid)} hashes calculated in {s.ElapsedMilliseconds}ms.");

            
        }

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
                _originalOut.WriteLine($"[{Now:hh:mm:sstt}] {message}");
                _logfile.WriteLine($"[{Now:hh:mm:sstt}] {message}");
            }

            public override void WriteLine(double message)
            {
               WriteLine(message.ToString(CultureInfo.InvariantCulture));
            }


            public override void WriteLine(float message)
            {
                WriteLine(message.ToString(CultureInfo.InvariantCulture));
            }

            public override void WriteLine(int message)
            {
                WriteLine(message.ToString(CultureInfo.InvariantCulture));
            }

            public override Encoding Encoding => new ASCIIEncoding();
        }
    }

    
}
