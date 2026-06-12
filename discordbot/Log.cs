using System.Runtime.CompilerServices;

namespace discordbot;

public class Log
{
    public static void Info(string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
    {
        Write("INFO", message, member, file, ConsoleColor.Green);
    }

    public static void Debug(string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
    {
        #if(DEBUG)
            Write("DEBUG", message, member, file, ConsoleColor.Cyan);
        #endif
    }

    public static void Warn(string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
    {
        Write("WARN", message, member, file, ConsoleColor.Yellow);
    }

    public static void Error(string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "")
    {
        Write("ERROR", message, member, file, ConsoleColor.Red);
    }

    public static void Error(Exception ex)
    {
        Error(ex.ToString());
    }

    private static void Write(string level, string message, string member, string file, ConsoleColor color)
    {
        var className = Path.GetFileNameWithoutExtension(file);

        Console.Write("[");
    
        Console.ForegroundColor = color;
        Console.Write(level);

        Console.ResetColor();

        Console.Write($"] [{className}.{member}] {message}");
        Console.WriteLine();
    }
}