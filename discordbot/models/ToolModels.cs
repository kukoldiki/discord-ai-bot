namespace discordbot.models;

public class ToolRequest
{
    public string Type { get; set; } = "function";
    public FunctionDefinition Function { get; set; } = new();
}

public class FunctionDefinition
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public FunctionParameters Parameters { get; set; } = new();
}

public class FunctionParameters
{
    public string Type { get; set; } = "object";
    public Dictionary<string, PropertyDefinition> Properties { get; set; } = new();
    public List<string> Required { get; set; } = new();
}

public class PropertyDefinition
{
    public string Type { get; set; } = "string";
    public string Description { get; set; } = "";
}