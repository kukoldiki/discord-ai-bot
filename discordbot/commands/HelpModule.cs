using System.Text;
using Discord.Commands;

namespace discordbot.commands;

public class HelpModule(CommandService commands) : ModuleBase<SocketCommandContext>
{
    [Command("help")]
    [Summary("Displays help")]
    public async Task Help()
    {
        var commands1 = commands.Commands;

        var sb = new StringBuilder();

        sb.AppendLine("```");
        
        foreach(var cmd in commands1)
        {
            var args = string.Join(" ", cmd.Parameters.Select(p => $"<{p.Name}>"));
            sb.AppendLine($"{cmd.Name} {args} - {cmd.Summary ?? "No description"}");
        }
        sb.AppendLine("```");
        
        await ReplyAsync(sb.ToString());
    }
}