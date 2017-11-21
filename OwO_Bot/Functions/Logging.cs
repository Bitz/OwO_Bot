using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string directory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            if (directory == null) return String.Empty;
            directory = Path.Combine(directory, "Logs");
            return directory;
        }
    }
}
