namespace discordbot.models;

public class AiModel(string name, bool thinking, bool vision, bool tools)
{
    public string Name { get; set; } = name;
    public bool Thinking { get; set; } = thinking;
    
    public bool Vision { get; set; } = vision;
    
    public bool Tools { get; set; } = tools;
}