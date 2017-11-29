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

        public static string WorkingSub { get; set; }

        #endregion Configuration

        public static string Version => Assembly.GetEntryAssembly().GetName().Version.ToString();

        //If this value changes, be sure to regenerate ALL entries in the database. 
        public static int PixelSize => 24;
    }
}
