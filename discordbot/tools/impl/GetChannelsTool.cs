using System.Text;
using Discord;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class GetChannelsTool : ITool
{
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "get_channels",
            Description = "Get channels in current guild"
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        var sbChannels = new StringBuilder();
        foreach (var channel in ctx.Guild.TextChannels)
        {
            if(channel is IThreadChannel) continue;
            sbChannels.AppendLine($"{channel.Name} - {channel.Id}");
        }

        return sbChannels.ToString();
    }
}