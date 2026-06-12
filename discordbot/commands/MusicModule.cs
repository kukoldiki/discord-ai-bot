using System.Diagnostics;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Victoria;

namespace discordbot.commands;

public class MusicModule : ModuleBase<SocketCommandContext>
{
    private static IAudioClient _audioClient;
    private readonly LavaNode _lavaNode;
    
    public MusicModule(LavaNode lavaNode)
    {
        _lavaNode = lavaNode;
    }
    
    [Command("join", RunMode = RunMode.Async)]
    public async Task Join(IVoiceChannel channel = null)
    {
        try
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Context.Channel.SendMessageAsync(
                    "User must be in a voice channel, or a voice channel must be passed as an argument.");
                return;
            }

            var audioClient = await channel.ConnectAsync();
            _audioClient = audioClient;
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }
    
    private Process CreateStream(string path)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        });
    }
    
    private string GetAudioUrl(string url)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp.exe",
                Arguments = $"-f bestaudio -g \"{url}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return output.Trim();
    }
    
    private async Task PlayAsync(IAudioClient client, string filePath)
    {
        using var ffmpeg = CreateStream(filePath);
        using var output = ffmpeg.StandardOutput.BaseStream;
        using var discord = client.CreatePCMStream(AudioApplication.Mixed);

        try
        {
            await output.CopyToAsync(discord);
        }
        finally
        {
            await discord.FlushAsync();
        }
    }
    
    /*[Command("play", RunMode = RunMode.Async)]
    public async Task Play(string path)
    {
        if (_audioClient == null)
        {
            await ReplyAsync("Join voice first");
            return;
        }

        await PlayAsync(_audioClient, path);
    }*/
    [Command("play", RunMode = RunMode.Async)]
    public async Task Play(string url)
    {
        if (_audioClient == null)
        {
            await ReplyAsync("Join voice first");
            return;
        }

        var streamUrl = GetAudioUrl(url);

        await PlayAsync(_audioClient, streamUrl);
    }
}