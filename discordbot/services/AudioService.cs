using System.Collections.Concurrent;
using System.Text.Json;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Victoria;
using Victoria.Enums;
using Victoria.WebSocket.EventArgs;

namespace discordbot;

public sealed class AudioService {
        private readonly LavaNode _lavaNode;
        private readonly DiscordSocketClient _socketClient;
        public readonly HashSet<ulong> VoteQueue;
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
        public readonly ConcurrentDictionary<ulong, ulong> TextChannels;
        
        public AudioService(
            LavaNode lavaNode,
            DiscordSocketClient socketClient) {
            _lavaNode = lavaNode;
            _socketClient = socketClient;
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
            TextChannels = new ConcurrentDictionary<ulong, ulong>();
            VoteQueue = [];
            _lavaNode.OnWebSocketClosed += OnWebSocketClosedAsync;
            _lavaNode.OnStats += OnStatsAsync;
            _lavaNode.OnPlayerUpdate += OnPlayerUpdateAsync;
            _lavaNode.OnTrackEnd += OnTrackEndAsync;
            _lavaNode.OnTrackStart += OnTrackStartAsync;
        }
        
        private Task OnTrackStartAsync(TrackStartEventArg arg) {
            var track = arg.Track;
            return SendAndLogMessageAsync(
                arg.GuildId,
                $"Now playing: {track.Title} - {track.Author} ({track.Duration.Minutes}:{track.Duration.Seconds:D2})"
            );
        }
        
        private async Task OnTrackEndAsync(TrackEndEventArg arg) {
            var player = await _lavaNode.GetPlayerAsync(arg.GuildId);
            if (arg.Reason == TrackEndReason.Finished && player != null && player.GetQueue().Count > 0)
            {
                var next = player.GetQueue().TryDequeue(out var track);
                await player.PlayAsync(_lavaNode, track);
            }
            await SendAndLogMessageAsync(arg.GuildId, $"{arg.Track.Title} ended with reason: {arg.Reason}");
        }
        
        private Task OnPlayerUpdateAsync(PlayerUpdateEventArg arg) {
            Log.Info($"Guild latency: {arg.Ping}ms");
            return Task.CompletedTask;
        }
        
        private Task OnStatsAsync(StatsEventArg arg) { 
            Log.Info("{}", JsonSerializer.Serialize(arg));
            return Task.CompletedTask;
        }
        
        private Task OnWebSocketClosedAsync(WebSocketClosedEventArg arg) {
            Log.Error("{}", JsonSerializer.Serialize(arg));
            return Task.CompletedTask;
        }
        
        private Task SendAndLogMessageAsync(ulong guildId,
                                            string message) {
            Log.Info(message);
            if (!TextChannels.TryGetValue(guildId, out var textChannelId)) {
                return Task.CompletedTask;
            }
            
            return (_socketClient
                    .GetGuild(guildId)
                    .GetChannel(textChannelId) as ITextChannel)
                .SendMessageAsync(message);
        }
    }