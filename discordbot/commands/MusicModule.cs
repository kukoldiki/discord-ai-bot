using System.Diagnostics;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Victoria;
using Victoria.Rest.Search;

namespace discordbot.commands;

public class MusicModule : ModuleBase<SocketCommandContext>
{
    private static IAudioClient _audioClient;
    private readonly LavaNode _lavaNode;
    private readonly AudioService _audioService;
    
    public MusicModule(LavaNode lavaNode, AudioService audioService)
    {
        _lavaNode = lavaNode;
        _audioService = audioService;
        
    }
    
    [Command("join")]
    public async Task JoinAsync() {
        var voiceState = Context.User as IVoiceState;
        if (voiceState?.VoiceChannel == null) {
            await ReplyAsync("You must be connected to a voice channel!");
            return;
        }
        
        try {
            await _lavaNode.JoinAsync(voiceState.VoiceChannel);
            await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            
            _audioService.TextChannels.TryAdd(Context.Guild.Id, Context.Channel.Id);
        }
        catch (Exception exception) {
            await ReplyAsync(exception.ToString());
        }
    }

    [Command("play")]
    public async Task PlayAsync([Remainder] string searchQuery) {
        if (string.IsNullOrWhiteSpace(searchQuery)) {
            await ReplyAsync("Please provide search terms.");
            return;
        }
        
        var player = await _lavaNode.TryGetPlayerAsync(Context.Guild.Id);
        if (player == null) {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            
            try {
                player = await _lavaNode.JoinAsync(voiceState.VoiceChannel);
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
                _audioService.TextChannels.TryAdd(Context.Guild.Id, Context.Channel.Id);
            }
            catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }
        
        var searchResponse = await _lavaNode.LoadTrackAsync(searchQuery);
        if (searchResponse.Type is SearchType.Empty or SearchType.Error) {
            await ReplyAsync($"I wasn't able to find anything for `{searchQuery}`.");
            return;
        }
        
        var track = searchResponse.Tracks.FirstOrDefault();
        if (player.Track == null) {
            await player.PlayAsync(_lavaNode, track);
            // await ReplyAsync($"Now playing: {track.Title}");
            return;
        }
        
        player.GetQueue().Enqueue(track);
        await ReplyAsync($"Added {track.Title} to queue.");
    }
    
    [Command("leave")]
    public async Task LeaveAsync() {
        var voiceChannel = (Context.User as IVoiceState).VoiceChannel;
        if (voiceChannel == null) {
            await ReplyAsync("Not sure which voice channel to disconnect from.");
            return;
        }
        
        try {
            await _lavaNode.LeaveAsync(voiceChannel);
            await ReplyAsync($"I've left {voiceChannel.Name}!");
        }
        catch (Exception exception) {
            await ReplyAsync(exception.Message);
        }
    }
    
    [Command("stop")]
    public async Task StopAsync() {
        var player = await _lavaNode.TryGetPlayerAsync(Context.Guild.Id);
        if(player == null)
            return;
        if (!player.State.IsConnected || player.Track == null) {
            await ReplyAsync("Woah, can't stop won't stop.");
            return;
        }
        
        try {
            await player.StopAsync(_lavaNode, player.Track);
            await ReplyAsync("No longer playing anything.");
        }
        catch (Exception exception) {
            await ReplyAsync(exception.Message);
        }
    }
    
    [Command("skip")]
    public async Task SkipAsync() {
        try
        {
            var player = await _lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if(player == null)
                return;
            if (!player.State.IsConnected)
            {
                await ReplyAsync("Woaaah there, I can't skip when nothing is playing.");
                return;
            }

            var queue = player.GetQueue();
            Log.Info("Queue: " + queue.Count);
            if (!queue.TryDequeue(out var next))
                return;
            
            Log.Info("Next " + next.Title);

            //await player.StopAsync(_lavaNode, player.Track);
            await player.PlayAsync(_lavaNode, next, noReplace: false);
        }
        catch (Exception exception) {
            await ReplyAsync(exception.Message);
            Log.Error(exception);
        }
    }
}