using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;

namespace discordbot;

public class Utils
{
    public static List<string> SplitByLength(string text, int maxLength)
    {
        var result = new List<string>();

        for (int i = 0; i < text.Length; i += maxLength)
        {
            result.Add(text.Substring(i, Math.Min(maxLength, text.Length - i)));
        }

        return result;
    }

    public static string GetTextFromHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        doc.DocumentNode.SelectNodes("//script|//style")?
            .ToList()
            .ForEach(n => n.Remove());
        return Regex.Replace(doc.DocumentNode.InnerText, @"\s+", " ").Trim();
    }
    
    public static string ReplaceChannels(string content, IReadOnlyCollection<SocketTextChannel> channels)
    {
        foreach (var channel in channels)
        {
            content = content.Replace(
                $"<#{channel.Id}>",
                $"#{channel.Name}"
            );
        }

        return content;
    }
}