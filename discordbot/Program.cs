using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using discordbot.models;
using discordbot.profiles;
using discordbot.tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
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
        
        NpgsqlConnection.GlobalTypeMapper.UseVector();

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

        var services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton(new CommandConfig()
            {
                AvailableModels =
                [
                    new("llava", false, true, false),
                    new("llama3.1", false, false, true),
                    new("gemma3", false, true, false),
                    new("mistral:latest", false, false, false),
                    new("gemma4:31b-cloud", true, true, true),
                    new("gpt-oss:20b-cloud", true, false, false),
                    new("gpt-oss:120b-cloud", true, false, true),
                    new("qwen2.5-coder:7b", false, false, true),
                    new("qwen3:8b", true, false, true),
                    new("gemma4:e4b", false, true, true),
                    new("nemotron-3-nano:30b-cloud", true, false, true),
                    new("rnj-1", false, false, true),
                    new("rnj-1:8b-cloud", false, false, true)
                    //new ("qwen3-coder-next:cloud", false, false, true)
                ],
                searxngAddress = "http://localhost:1852/"
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
            .AddSingleton<ProfileRegistry>()
            .AddSingleton<HttpClient>(_ =>
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(config["ollamaBaseUrl"] ?? "http://localhost:11434"),
                    Timeout = TimeSpan.FromMinutes(5)
                };
                client.DefaultRequestHeaders.Add("User-Agent", $"discordbotai/1.0 ({config["mail"] ?? "null"})");
                return client;
            });

        var toolTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ITool).IsAssignableFrom(t));

        foreach (var type in toolTypes)
            services.AddSingleton(typeof(ITool), type);

        services.AddSingleton<ToolRegistry>();
        
        _services = services.BuildServiceProvider();
        
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