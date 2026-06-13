namespace discordbot.models;

public class AiModel(string name, bool thinking)
{
    public string Name { get; set; } = name;
    public bool Thinking { get; set; } = thinking;
}