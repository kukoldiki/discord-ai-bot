using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using discordbot.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace discordbot;
// $"Host=${config["db:host"] ?? "localhost"};Port={config["db:port"] ?? "5432"};Database={config["db:db"] ?? "botdb"};Username={config["db:user"] ?? "bot"};Password={config["password"] ?? "pass"}"
class Program
{
    private static DiscordSocketClient _client;
    private static CommandHandler _commandHandler;
    private static CommandService _commands;
    
    public static async Task Main(string[] args)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        var socketConfig = new DiscordSocketConfig
        {
            GatewayIntents =
                GatewayIntents.Guilds |
                GatewayIntents.GuildMessages |
                GatewayIntents.MessageContent
        };
        
        _client = new DiscordSocketClient(socketConfig);

        _client.Log += LogMessage;
        
        await _client.LoginAsync(TokenType.Bot, config["token"]);
        await _client.StartAsync();
        
        _commands = new CommandService();
        
        var services = new ServiceCollection()
            .AddSingleton(new CommandConfig()
            {
                AvailableModels = ["gemma3", "mistral:latest", "gemma4:31b-cloud", "gpt-oss:120b-cloud", "qwen2.5-coder:7b"]
            })
            .AddSingleton(_commands)
            .AddSingleton(
                new Db(
                    $"Host={config["db:host"] ?? "localhost"};" +
                    $"Port={config["db:port"] ?? "5432"};" +
                    $"Database={config["db:db"] ?? "botdb"};" +
                    $"Username={config["db:user"] ?? "bot"};" +
                    $"Password={config["password"] ?? "pass"}"
                )
                )
            .AddSingleton<ChatHistoryService>()
            .AddSingleton<HttpClient>(sp =>
                {
                    return new HttpClient
                    {
                        BaseAddress = new Uri(config["ollamaBaseUrl"] ?? "http://localhost:11434"),
                    };
                })
            .BuildServiceProvider();

        _commandHandler = new CommandHandler(_client, _commands, config["prefix"] ?? "!", services);
        
        await _commandHandler.InstallCommandsAsync();
        
        await Task.Delay(-1);
    }

    public static async Task LogMessage(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Info:
                Log.Info(message.Message);
                break;
            case LogSeverity.Error:
            case LogSeverity.Critical:
                Log.Error(message.Message + "\n" + message.Exception);
                break;
            case LogSeverity.Debug:
            case LogSeverity.Verbose:
                Log.Debug(message.Message);
                break;
            case LogSeverity.Warning:
                Log.Warn(message.Message);
                break;
            default:
                Log.Warn("Unknown log severity " + message.Severity + " " + message.Message);
                break;
        }
    }
}