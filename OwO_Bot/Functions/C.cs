using Con = System.Console;
using static System.DateTime;

namespace OwO_Bot.Functions
{
    public static class C
    {
        public static void WriteLine(object message){
            Con.WriteLine($"[{Now:hh:mm:sstt}] {message as string}");
        }

        public static void Write(object message)
        {
            Con.Write($"[{Now:hh:mm:sstt}] {message as string}");
        }

        public static void WriteNoTime(object message)
        {
            Con.Write(message as string);
        }

        public static void WriteLineNoTime(object message)
        {
            Con.WriteLine(message as string);
        }

    }
}
