using Discord.Commands;

namespace discordbot.commands;

public class PingModule : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    public async Task PingAsync()
    {
        await ReplyAsync("pong!");
    }
}