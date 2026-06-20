using discordbot.models;

namespace discordbot;

public class CommandConfig
{
    // public HttpClient ollamaClient { get; set; }
    public AiModel[] AvailableModels { get; set; }
    public string SearxngAddress { get; set; }
    
    public string ExecServerBaseUrl { get; set; }
}