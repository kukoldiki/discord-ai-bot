using discordbot.models;

namespace discordbot.tools;

public class ToolRegistry
{
    private readonly Dictionary<string, ITool> _tools;

    public ToolRegistry(IEnumerable<ITool> tools)
    {
        _tools = tools.ToDictionary(t => t.Definition.Function.Name);
    }

    public ITool? Get(string name) => _tools.GetValueOrDefault(name);

    public List<ToolRequest> GetDefinitions(IEnumerable<string> names)
    {
        if (names.Any(n => n.Equals("all", StringComparison.OrdinalIgnoreCase)))
            return _tools.Values.Select(t => t.Definition).ToList();

        return names
            .Where(_tools.ContainsKey)
            .Select(n => _tools[n].Definition)
            .ToList();
    }
}