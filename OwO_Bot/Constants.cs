using System.Reflection;
using OwO_Bot.Models;

namespace OwO_Bot
{
    static class Constants
    {
        #region Configuration
        private static configuration _config;

        public static configuration Config
        {
            get => _config ?? (_config = Functions.Configuration.Load());
            set
            {
                _config = value;
                Functions.Configuration.Save(value);
            }
        }

        public static string[] Args { get; set; }

        public static string WorkingSub { get; set; }

        public static bool MailBasedTitle { get; set; } = false;

        public static configurationMailReciever EmailRecipient { get; set; }
        #endregion Configuration

        public static string Version => Assembly.GetEntryAssembly().GetName().Version.ToString();

        //If this value changes, be sure to regenerate ALL entries in the database. 
        public static int PixelSize => 24;

        //List of Tags that should never end up on reddit or will not be handled by the bot.
        public static string[] TagsToHide = {"loli", "cub", "shota", "chibi"};
    }
}
