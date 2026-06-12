using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace discordbot;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly String _prefix;
    private readonly ServiceProvider _services;
    
    public CommandHandler(DiscordSocketClient client, CommandService commands, String prefix, ServiceProvider services)
    {
        _commands = commands;
        _client = client;
        _prefix = prefix;
        _services = services;
    }
    
    public async Task InstallCommandsAsync()
    {
        _client.MessageReceived += HandleCommandAsync;
        
        await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), 
            services: _services);
    }
    
    private Task HandleCommandAsync(SocketMessage messageParam)
    {
        _ = ProcessMessageAsync(messageParam);
        return Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(SocketMessage messageParam)
    {
        var message = messageParam as SocketUserMessage;
        if (message == null || message.Author.IsBot) return;
        
        var content = message.Content;
        int argPos = 0;
        
        Log.Debug("Message content " + content);

        if (!(message.HasStringPrefix(_prefix, ref argPos) ||
              message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
            message.Author.IsBot)
        {
            Log.Debug("Skipping message");
            return;
        }

        var context = new SocketCommandContext(_client, message);

        await _commands.ExecuteAsync(
            context: context, 
            argPos: argPos,
            services: _services);
    }
}