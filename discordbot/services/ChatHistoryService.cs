using System.Collections.Concurrent;

namespace discordbot.models;


public class ChatHistoryService
{
    private readonly ConcurrentDictionary<ulong, List<ChatMessage>> _history = new();
    private readonly ConcurrentDictionary<ulong, ToolState> _toolStates = new();

    public List<ChatMessage> GetHistory(ulong userId)
    {
        return _history.GetOrAdd(userId, _ => []);
    }

    public ToolState GetToolState(ulong userId)
    {
        return _toolStates.GetOrAdd(userId, _ => new ToolState());
    }
}

public class ToolState
{
    public int ToolCallCount;
}
public class ChatMessage
{
    public string Role { get; set; }
    public string Content { get; set; }

    public string? ToolName { get; set; } // tool name
    
    public List<ToolCall>? ToolCalls { get; set; }
}