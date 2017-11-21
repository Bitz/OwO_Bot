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
        #endregion Configuration

        public static string Version => Assembly.GetEntryAssembly().GetName().Version.ToString();
    }
}
