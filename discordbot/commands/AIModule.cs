using System.Net.Http.Json;
using Discord.Commands;
using discordbot.models;
using Microsoft.Extensions.Logging;

namespace discordbot.commands;

public class AIModule : ModuleBase<SocketCommandContext>
{
    private readonly HttpClient _ollamaClient;
    private readonly Db _db;
    private readonly CommandConfig _config;
    
    public AIModule(HttpClient ollamaClient, Db db, CommandConfig config)
    {
        _ollamaClient = ollamaClient;
        _db = db;
        _config = config;
        Log.Debug($"DB Initialized! ${db}");
    }

    [Command("prompt")]
    public async Task prompt([Remainder] string input)
    {
        try
        {
            var settings = await _db.GetOrCreateUserSettings((long)Context.User.Id);
            settings.SystemPrompt = $"{input}\n\nYour answer should be a maximum of 1999 characters!";
            await _db.UpdateUserSettings(settings);
            await ReplyAsync("Done!");
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
    }

    [Command("settings")]
    public async Task settings()
    {
        try
        {
            var settings = await _db.GetOrCreateUserSettings((long)Context.User.Id);
            await ReplyAsync($"User settings:\nModel: {settings.Model}\n\nSystem prompt: {settings.SystemPrompt}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Command("models")]
    public async Task models()
    {
        try
        {
            await ReplyAsync(String.Join(", ", _config.AvailableModels));
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
    }

    [Command("model")]
    public async Task model([Remainder] string input)
    {
        try
        {
            if (!_config.AvailableModels.Contains(input))
            {
                await ReplyAsync("Model not found! Try run models command");
                return;
            }
            var settings = await _db.GetOrCreateUserSettings((long)Context.User.Id);
            settings.Model = input;
            await _db.UpdateUserSettings(settings);
            await ReplyAsync("Done!");
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
    }
    

    [Command("ask")]
    [Summary("Ask smart AI.")]
    public async Task ask([Remainder] string input)
    {
        try
        {
            var settings = await _db.GetOrCreateUserSettings((long)Context.User.Id);
            
            var data = new
            {
                model = settings.Model,
                stream = false,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = settings.SystemPrompt
                    },
                    new
                    {
                        role = "user",
                        content = input
                    }
                }
            };
            
            await Context.Channel.TriggerTypingAsync();
            
            var response = await _ollamaClient.PostAsJsonAsync("/api/chat", data);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                await ReplyAsync("Error, try again later or change model");
                Log.Error($"Failed to ask model!\n${await response.Content.ReadAsStringAsync()}");
                return;
            }
            var obj = await response.Content.ReadFromJsonAsync<ChatApiResponse>();

            var aiResponse = obj?.Message.Content ?? "No response";

            if (aiResponse.Length > 1999)
            {
                aiResponse = aiResponse.Substring(0, 1999);
            }
            
            await ReplyAsync(aiResponse);
        }
        catch (Exception e)
        {
            await ReplyAsync("Error, try again later or change model");
            Log.Error($"{e.Message}\n{e.StackTrace}");
        }
    }
}