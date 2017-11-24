using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using OwO_Bot.Models;

namespace OwO_Bot.Functions
{
    class Configuration
    {
        public static configuration Load()
        {
            XmlDocument config = new XmlDocument();
            config.Load(PathToConfig());
            XmlNode node = config.DocumentElement;
            return Convert.NodeToClass<configuration>(node);
        }
        
        /// <summary>
        /// Users will have to call this method to save changes, as changing values in the model alone will not cause the file to reflect these changes.
        /// This is to allow temporary values that may change at runtime.
        /// </summary>
        /// <param name="saveMe">We may specificy a model to be saved instead of the one in the constants class</param>
        /// <returns></returns>
        public static bool Save(configuration saveMe = null)
        {
            if (saveMe == null)
            {
                saveMe = Constants.Config;
            }
            string path = PathToConfig();
            XmlDocument config = new XmlDocument();
            try
            {
                config.Load(path);
                XmlSerializer xs = new XmlSerializer(typeof(configuration));
                
                using (MemoryStream stream = new MemoryStream())
                {
                    xs.Serialize(stream, saveMe);
                    stream.Position = 0;
                    config.Load(stream);
                    config.Save(path);
                    stream.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string PathToConfig()
        {
            string directory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            if (directory == null) return String.Empty;
            directory = Path.Combine(directory, "configuration.xml");
            return directory;
        }
    }
}
