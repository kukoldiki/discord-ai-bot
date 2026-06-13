using Discord;
using Discord.Commands;
using Discord.WebSocket;
using discordbot.models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Victoria;

namespace discordbot;

class Program
{
    private static DiscordSocketClient _client;
    private static CommandHandler _commandHandler;
    private static CommandService _commands;

    private static LavaNode _lavaNode;
    private static Configuration _lavaConfig;
    private static ServiceProvider _services;

    public static async Task Main(string[] args)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var socketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All
        };

        _client = new DiscordSocketClient(socketConfig);

        _client.Log += LogMessage;
        
        _lavaConfig = new Configuration
        {
            Hostname = "127.0.0.1",
            Port = 8080,
            Authorization = "youshallnotpass",
            SelfDeaf = true
        };

        _lavaNode = new LavaNode(
            _client,
            _lavaConfig,
            NullLogger<LavaNode>.Instance
        );
        
        _commands = new CommandService();

        _services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton(new CommandConfig()
            {
                AvailableModels =
                [
                    new("gemma3", false),
                    new("mistral:latest", false),
                    new("gemma4:31b-cloud", true),
                    new("gpt-oss:20b-cloud", true),
                    new("gpt-oss:120b-cloud", true),
                    new("qwen2.5-coder:7b", false),
                    new("qwen3:8b", true),
                    new("gemma4:e4b", false)
                ]
            })
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
            .AddSingleton<AudioService>()
            .AddSingleton(_lavaNode)
            .AddSingleton<HttpClient>(_ =>
            {
                return new HttpClient
                {
                    BaseAddress = new Uri(config["ollamaBaseUrl"] ?? "http://localhost:11434"),
                };
            })
            .BuildServiceProvider();

        _commandHandler = new CommandHandler(_client, _commands, config["prefix"] ?? "!", _services);

        await _commandHandler.InstallCommandsAsync();
        
        _client.Ready += async () =>
        {
            Console.WriteLine("Discord READY → connecting Lavalink...");
            await _lavaNode.ConnectAsync();
        };

        await _client.LoginAsync(TokenType.Bot, config["token"]);
        await _client.StartAsync();

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