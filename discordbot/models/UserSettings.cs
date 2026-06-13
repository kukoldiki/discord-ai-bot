using System.ComponentModel.DataAnnotations;

namespace discordbot.models;

public class UserSettings
{
    [Key]
    public long UserId { get; set; }

    public string Model { get; set; } = "gemma4:31b-cloud";
    public string SystemPrompt { get; set; } = "You are helpful assistant";
    
    public bool Thinking { get; set; } = false;
}