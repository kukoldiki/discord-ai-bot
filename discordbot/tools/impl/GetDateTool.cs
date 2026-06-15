using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class GetDateTool : ITool
{
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "get_date",
            Description = "Get current date"
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext context)
    {
        return DateTimeOffset.UtcNow.ToString();
    }
}