namespace discordbot.profiles;

public class AiProfile
{
    public string Name { get; set; }
    public string SystemPrompt { get; set; }
    public List<string> AllowedTools { get; set; } = new();
}