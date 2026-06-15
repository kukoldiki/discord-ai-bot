using System.Text;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class GetGuildsTool : ITool
{
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "get_guilds",
            Description = "Get available guilds"
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        var sbGuilds = new StringBuilder();
        foreach (var guild in ctx.Client.Guilds)
        {
            sbGuilds.AppendLine($"{guild.Name} - {guild.Id}");
        }
        return sbGuilds.ToString();
    }
}