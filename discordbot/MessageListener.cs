using Discord;
using Discord.WebSocket;

namespace discordbot;

public class MessageListener
{
    private static readonly Dictionary<List<String>, String> _messageContents = new()
    {
        { new() {"мур", "мррр"}, "муррр~" },
        { new() {"мяу", "uwu", "nya"}, "ня~"},
        { new() {"гав"}, "good boy"}
    };
    public static async Task ProcessMessageAsync(SocketMessage messageParam)
    {
        var message = messageParam as SocketUserMessage;
        if (message == null || message.Author.IsBot) return;
        var content = message.Content.ToLowerInvariant();

        foreach (var (triggers, response) in _messageContents)
        {
            if (triggers.Any(trigger => content.Contains(trigger.ToLowerInvariant())))
            {
                await message.ReplyAsync(response, allowedMentions: AllowedMentions.None);
                return;
            }
        }
    }
}