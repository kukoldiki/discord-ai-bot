using Discord;
using Discord.Commands;

namespace discordbot.commands;

public class OwnerModule : ModuleBase<SocketCommandContext>
{
    [Command("exportemoji")]
    public async Task Exportemoji([Remainder] string nya)
    {
        try
        {
            if (Context.User.Id != 1437867795542180041)
            {
                await ReplyAsync("You are not the owner of the bot.");
                return;
            }

            var from = Context.Client.GetGuild(ulong.Parse(nya));
            using var http = new HttpClient();
            foreach (var emote in from.Emotes)
            {
                await using var stream = await http.GetStreamAsync(emote.Url);
                await Context.Guild.CreateEmoteAsync(emote.Name, new Image(stream));
                await ReplyAsync($"{emote.Name} exported as {emote.Url}");
            }
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }
}