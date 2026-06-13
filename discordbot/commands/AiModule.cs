using System.Net.Http.Json;
using Discord;
using Discord.Commands;
using discordbot.models;
using discordbot;

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

    [Command("think")]
    [Summary("Enable/Disable thinking (when possible)")]
    public async Task Think()
    {
        try
        {
            var settings =  await _db.GetOrCreateUserSettings((long)Context.User.Id);
            settings.Thinking = !settings.Thinking;
            await _db.UpdateUserSettings(settings);
            await ReplyAsync($"Done! New value is {settings.Thinking}");
        }
        catch  (Exception e)
        {
            await ReplyAsync("Failed! Try again later");
            Log.Error(e);
        }
    }

    [Command("prompt")]
    [Summary("Set system prompt")]
    public async Task Prompt([Remainder] string input)
    {
        try
        {
            var settings = await _db.GetOrCreateUserSettings((long)Context.User.Id);
            settings.SystemPrompt = input;
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
    [Summary("Ask AI without saving to history. NO THINKING!")]
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
            var tokensStr = $"`In: {obj?.PromptEvalCount ?? 0} {inputTps:f1}T/s | Out: {obj?.EvalCount ?? 0} {outputTps:f1}T/s`";

            if (aiResponse.Length > maxMessageLength)
            {
                var parts = Utils.SplitByLength(aiResponse, maxMessageLength);
                IThreadChannel? thread = null;
                foreach (var part in parts)
                {
                    if (thread == null)
                    {
                        var message = await ReplyAsync($"{part}\n\n{tokensStr}");
                        if (Context.Channel is ITextChannel textChannel)
                        {
                            thread = await textChannel.CreateThreadAsync(
                                name: "Response",
                                message: message
                            );
                        }
                    }
                    else
                    {
                        await thread.SendMessageAsync(part);
                    }
                }
            }
            else
            {
                await ReplyAsync($"{aiResponse}\n\n{tokensStr}");
            }
        }
        catch (Exception e)
        {
            await ReplyAsync("Error, try again later or change model");
            Log.Error(e.Message);
        }
    }

    [Command("ask")]
    [Summary("Ask smart AI. With history saving.")]
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
                messages,
                think = settings.Thinking
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
            
            Log.Info($"{await response.Content.ReadAsStringAsync()}");
            
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

            var tokensStr = $"`In: {obj?.PromptEvalCount ?? 0} {inputTps:f1}T/s | Out: {obj?.EvalCount ?? 0} {outputTps:f1}T/s`";

            IThreadChannel? thread = null;
            IUserMessage? message = null;
            
            if (aiResponse.Length > maxMessageLength)
            {
                var parts = Utils.SplitByLength(aiResponse, maxMessageLength);
                foreach (var part in parts)
                {
                    if (thread == null)
                    {
                        message = await ReplyAsync($"{part}\n\n{tokensStr}");
                        if (Context.Channel is ITextChannel textChannel)
                        {
                            thread = await textChannel.CreateThreadAsync(
                                name: "Response",
                                message: message
                            );
                        }
                    }
                    else
                    {
                        await thread.SendMessageAsync(part);
                    }
                }
            }
            else
            {
                await ReplyAsync($"{aiResponse}\n\n{tokensStr}");
            }
            if(obj.Message.Thinking.Length == 0 || message == null)
                return;
            if (thread == null)
            {
                if (Context.Channel is ITextChannel textChannel)
                {
                    thread = await textChannel.CreateThreadAsync(
                        name: "Thinking",
                        message: message
                    );
                }
                else
                {
                    return;
                }
            }

            var thinkStr = obj.Message.Thinking;
            if (thinkStr.Length > maxMessageLength)
            {
                thinkStr = thinkStr.Substring(0, maxMessageLength);
            }

            await thread.SendMessageAsync($"Thinking:\n\n{thinkStr}");
        }
        catch (Exception e)
        {
            await ReplyAsync("Error, try again later or change model");
            Log.Error($"{e.Message}\n{e.StackTrace}");
        }
    }
}