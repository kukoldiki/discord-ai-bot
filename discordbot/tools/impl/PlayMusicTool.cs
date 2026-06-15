using System.Text;
using Discord;
using Discord.Commands;
using discordbot.models;
using Victoria;
using Victoria.Rest.Search;

namespace discordbot.tools.impl;

public class PlayMusicTool : ITool
{
    private readonly LavaNode _lavaNode;
    private readonly AudioService _audioService;

    public PlayMusicTool(LavaNode lavaNode, AudioService audioService)
    {
        _lavaNode = lavaNode;
        _audioService = audioService;
    }
    
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "play_music",
            Description = "Plays music",
            Parameters = new()
            {
                Properties = new()
                {
                    ["query"] = new()
                    {
                        Type = "string",
                        Description =
                            "Play some music. Only a link(youtube) or the name on YouTube Music. Multiple songs separated by newline or comma. 5 tracks max!"
                    }
                },
                Required = new() { "query" }
            }
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext context)
    {
        var rawQuery = func.Arguments.First().Value?.ToString();

        if (string.IsNullOrWhiteSpace(rawQuery))
        {
            return "Please provide search terms.";
        }

        var queries = rawQuery
            .Split(new[] { '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        if (queries.Count > 11)
        {
            return "Too many tracks!";
        }

        var voiceState = context.User as IVoiceState;
        if (voiceState?.VoiceChannel == null)
        {
            await context.Channel.SendMessageAsync("You must be connected to a voice channel!");
            return "User must be connected to a voice channel!";
        }

        var player = await _lavaNode.TryGetPlayerAsync(context.Guild.Id);
        if (player == null)
        {
            try
            {
                player = await _lavaNode.JoinAsync(voiceState.VoiceChannel);
                await context.Channel.SendMessageAsync($"Joined {voiceState.VoiceChannel.Name}!");
                _audioService.TextChannels.TryAdd(context.Guild.Id, context.Channel.Id);
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        var sb = new StringBuilder();

        foreach (var query in queries)
        {
            var searchQuery = query;

            if (!searchQuery.Contains("https://"))
            {
                searchQuery = "ytmsearch:" + searchQuery;
            }

            Log.Info("Searching for " + searchQuery);

            var searchResponse = await _lavaNode.LoadTrackAsync(searchQuery);
            if (searchResponse.Type is SearchType.Empty or SearchType.Error)
            {
                await context.Channel.SendMessageAsync($"I wasn't able to find anything for `{searchQuery}`.");
                sb.AppendLine($"{searchQuery} not found");
                continue;
            }

            var track = searchResponse.Tracks.FirstOrDefault();
            sb.AppendLine($"Added {track.Title} to queue.");
            if (player.Track == null)
            {
                await player.PlayAsync(_lavaNode, track);
                // await ReplyAsync($"Now playing: {track.Title}");
                await Task.Delay(500);
                continue;
            }

            player.GetQueue().Enqueue(track);
            await context.Channel.SendMessageAsync($"Added {track.Title} to queue.");
        }

        await context.Channel.SendMessageAsync($"Processed {queries.Count} track(s).");
        return sb.ToString();
    }
}