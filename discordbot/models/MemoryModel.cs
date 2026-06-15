namespace discordbot.models;

public class Memory
{
    public long Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public string Type { get; set; }
}