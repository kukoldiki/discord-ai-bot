using Discord.Commands;
using discordbot.models;

namespace discordbot.tools;

public interface ITool
{
    ToolRequest Definition { get; }
    Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx);
}