namespace discordbot.profiles;

public class ProfileRegistry
{
    public readonly Dictionary<string, AiProfile> _profiles = new();

    public ProfileRegistry()
    {
        Register(new AiProfile
        {
            Name = "assistant",
            SystemPrompt = "Ты полезный универсальный ассистент.",
            AllowedTools = new() { "all" }
        });

        Register(new AiProfile
        {
            Name = "coder",
            SystemPrompt = "Ты опытный разработчик. Пиши чистый код.",
            AllowedTools = new() { "execute", "run_python", "math", "http_get", "search_web", "save_memory", "get_memory" }
        });

        Register(new AiProfile
        {
            Name = "dj",
            SystemPrompt = "Ты музыкальный бот.",
            AllowedTools = new() { "play_music", "search_web", "http_get" }
        });
    }

    public void Register(AiProfile profile) => _profiles[profile.Name] = profile;
    public AiProfile? Get(string name) => _profiles.GetValueOrDefault(name);
    public IEnumerable<string> GetNames() => _profiles.Keys;
}