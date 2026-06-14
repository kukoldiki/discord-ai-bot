using System.Text.Json.Serialization;

namespace discordbot.models;

public class ChatApiResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("message")]
    public Message Message { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("done_reason")]
    public string DoneReason { get; set; }

    [JsonPropertyName("total_duration")]
    public long TotalDuration { get; set; }

    [JsonPropertyName("load_duration")]
    public long LoadDuration { get; set; }

    [JsonPropertyName("prompt_eval_count")]
    public int PromptEvalCount { get; set; }

    [JsonPropertyName("prompt_eval_duration")]
    public long PromptEvalDuration { get; set; }

    [JsonPropertyName("eval_count")]
    public int EvalCount { get; set; }

    [JsonPropertyName("eval_duration")]
    public long EvalDuration { get; set; }
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("thinking")] 
    public string Thinking { get; set; } = "";
    
    [JsonPropertyName("tool_calls")]
    public List<ToolCall>? ToolCalls { get; set; }
}

public class ToolCall
{
    [JsonPropertyName("function")]
    public ToolFunction Function { get; set; }
}

public class ToolFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("arguments")]
    public Dictionary<string, object> Arguments { get; set; }
}