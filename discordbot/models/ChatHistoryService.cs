using System.Collections.Concurrent;

namespace discordbot.models;


public class ChatHistoryService
{
    private readonly ConcurrentDictionary<ulong, List<ChatMessage>> _history = new();

    public List<ChatMessage> GetHistory(ulong userId)
    {
        return _history.GetOrAdd(userId, _ => []);
    }
}

public class ChatMessage
{
    public string Role { get; set; }
    public string Content { get; set; }
}