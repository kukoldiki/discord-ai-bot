using System.Text;
using Discord.Commands;

namespace discordbot.commands;

public class HelpModule : ModuleBase<SocketCommandContext>
{
    private readonly CommandService _commands;
    
    public  HelpModule(CommandService commands)
    {
        _commands = commands;
    }

    [Command("help")]
    [Summary("Displays help")]
    public async Task help()
    {
        var commands = _commands.Commands;

        var sb = new StringBuilder();

        sb.AppendLine("```");
        
        foreach(var cmd in commands)
        {
            var args = string.Join(" ", cmd.Parameters.Select(p => $"<{p.Name}>"));
            sb.AppendLine($"{cmd.Name} {args} - {cmd.Summary ?? "No description"}");
        }
        sb.AppendLine("```");
        
        await ReplyAsync(sb.ToString());
    }
}