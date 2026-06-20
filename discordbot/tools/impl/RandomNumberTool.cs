using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class RandomNumberTool : ITool
{
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "random_number",
            Description = "Returns a random number",
            Parameters = new()
            {
                Properties = new()
                {
                    ["min"] = new PropertyDefinition()
                    {
                        Type = "integer",
                        Description = "Minimal number"
                    },
                    ["max"] = new PropertyDefinition()
                    {
                        Type = "integer",
                        Description = "Maximal number"
                    }
                }
            },
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        if (!func.Arguments.TryGetValue("min", out var minStr))
        {
            return "min is missing";
        }

        if (!func.Arguments.TryGetValue("max", out var maxStr))
        {
            return "max is missing";
        }

        if (!int.TryParse(minStr.ToString(), out var min))
        {
            return "failed to parse min";
        }
        if (!int.TryParse(maxStr.ToString(), out var max))
        {
            return "failed to parse max";
        }
        return Random.Shared.Next(min, max).ToString();
    }
}