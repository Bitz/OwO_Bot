using System;
using System.IO;

namespace OwO_Bot.Functions
{
    class Logging
    {
        public static StreamWriter CreateNew()
        {
            string absoluteCurrentDirectory = PathToLog();
            string loggingpath = PathToLog();
            Directory.CreateDirectory(Path.Combine(absoluteCurrentDirectory, loggingpath));
            string fullloggingPath = Path.Combine(absoluteCurrentDirectory, loggingpath);
            string logName = $"{DateTime.Now:MM-dd-yyyy-hh-mm-ss-tt}.log";
            fullloggingPath = Path.Combine(fullloggingPath, logName);
            StreamWriter logfile = File.CreateText(fullloggingPath);
            logfile.AutoFlush = true;
            return logfile;
        }

        public static string PathToLog()
        {
            string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string absoluteCurrentDirectory = Path.GetDirectoryName(location);
            absoluteCurrentDirectory = Path.Combine(absoluteCurrentDirectory, "Logs");
            return absoluteCurrentDirectory;
        }
    }
}
