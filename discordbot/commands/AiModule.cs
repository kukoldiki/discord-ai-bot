using System.Net.Http.Json;
using Discord.Commands;
using discordbot.models;

namespace discordbot.commands;

public class AiModule : ModuleBase<SocketCommandContext>
{
    private readonly HttpClient _ollamaClient;
    private readonly Db _db;
    private readonly CommandConfig _config;
    private readonly ChatHistoryService _history;
    
    private static readonly int maxMessageLength = 1900;
    
    public AiModule(HttpClient ollamaClient, Db db, CommandConfig config, ChatHistoryService historyService)
    {
        _ollamaClient = ollamaClient;
        _db = db;
        _config = config;
        _history = historyService;
        Log.Debug($"DB Initialized! ${db}");
    }

    [Command("prompt")]
    [Summary("Set system prompt")]
    public async Task Prompt([Remainder] string input)
    {
        try
        {
            var settings = await _db.GetOrCreateUserSettings((long)Context.User.Id);
            settings.SystemPrompt = $"{input}\n\nYour answer should be a maximum of {maxMessageLength} characters!";
            await _db.UpdateUserSettings(settings);
            await ReplyAsync("Done!");
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
    }

    [Command("settings")]
    [Summary("Show user settings")]
    public async Task Settings()
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
    [Summary("Show all available models")]
    public async Task Models()
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
    [Summary("Set model")]
    public async Task Model([Remainder] string model)
    {
        try
        {
            if (!_config.AvailableModels.Contains(model))
            {
                await ReplyAsync("Model not found! Try run models command");
                return;
            }
            var settings = await _db.GetOrCreateUserSettings((long)Context.User.Id);
            settings.Model = model;
            await _db.UpdateUserSettings(settings);
            await ReplyAsync("Done!");
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
    }
    
    [Command("clear")]
    [Summary("Clear history")]
    public async Task ClearHistory()
    {
        try
        {
            _history.GetHistory(Context.User.Id).Clear();
            await ReplyAsync("Done!");
        }
        catch  (Exception e)
        {
            Log.Error(e.Message);
        }
    }

    [Command("btw")]
    [Summary("Ask AI without saving to history")]
    public async Task Btw([Remainder] string prompt)
    {
        try
        {
            var settings = await _db.GetOrCreateUserSettings((long)Context.User.Id);
            await Context.Channel.TriggerTypingAsync();
            var response = await _ollamaClient.PostAsJsonAsync("/api/generate", new
            {
                model = settings.Model,
                stream = false,
                prompt
            });
            
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                await ReplyAsync("Error, try again later or change model");
                Log.Error($"Failed to ask model!\n${await response.Content.ReadAsStringAsync()}");
                return;
            }
            var obj = await response.Content.ReadFromJsonAsync<GenerateApiResponse>();

            var aiResponse = obj?.Response ?? "No response";

            if (aiResponse.Length > maxMessageLength)
            {
                aiResponse = aiResponse.Substring(0, maxMessageLength);
            }
            
            var outputTps = 0.0;
            var inputTps = 0.0;

            if (obj != null)
            {
                if (obj.EvalDuration > 0)
                {
                    outputTps = obj.EvalCount / (obj.EvalDuration / 1_000_000_000.0);
                }

                if (obj.PromptEvalDuration > 0)
                {
                    inputTps = obj.PromptEvalCount / (obj.PromptEvalDuration / 1_000_000_000.0);
                }
            }
            
            await ReplyAsync($"{aiResponse}\n`In: {obj?.PromptEvalCount ?? 0} {inputTps:f1}T/S | Out: {obj?.EvalCount ?? 0} {outputTps:f1}T/S`");
        }
        catch (Exception e)
        {
            await ReplyAsync("Error, try again later or change model");
            Log.Error(e.Message);
        }
    }

    [Command("ask")]
    [Summary("Ask smart AI.")]
    public async Task Ask([Remainder] string prompt)
    {
        try
        {
            var settings = await _db.GetOrCreateUserSettings((long)Context.User.Id);
            var history = _history.GetHistory(Context.User.Id);
            
            var messages = new List<object>
            {
                new
                {
                    role = "system",
                    content = settings.SystemPrompt
                }
            };

            messages.AddRange(history.Select(x => new
            {
                role = x.Role,
                content = x.Content
            }));

            messages.Add(new
            {
                role = "user",
                content = prompt
            });
            
            var data = new
            {
                model = settings.Model,
                stream = false,
                messages
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

            if (aiResponse.Length > maxMessageLength)
            {
                aiResponse = aiResponse.Substring(0, maxMessageLength);
            }
            
            history.Add(new ChatMessage
            {
                Role = "user",
                Content = prompt
            });
            history.Add(new ChatMessage
            {
                Role = "assistant",
                Content = aiResponse
            });
            
            while (history.Count > 15)
            {
                history.RemoveAt(0);
            }

            var outputTps = 0.0;
            var inputTps = 0.0;

            if (obj != null)
            {
                if (obj.EvalDuration > 0)
                {
                    outputTps = obj.EvalCount / (obj.EvalDuration / 1_000_000_000.0);
                }

                if (obj.PromptEvalDuration > 0)
                {
                    inputTps = obj.PromptEvalCount / (obj.PromptEvalDuration / 1_000_000_000.0);
                }
            }

            await ReplyAsync($"{aiResponse}\n`In: {obj?.PromptEvalCount ?? 0} {inputTps:f1}T/S | Out: {obj?.EvalCount ?? 0} {outputTps:f1}T/S`");
        }
        catch (Exception e)
        {
            await ReplyAsync("Error, try again later or change model");
            Log.Error($"{e.Message}\n{e.StackTrace}");
        }
    }
}