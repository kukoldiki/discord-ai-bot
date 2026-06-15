using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class SaveMemoryTool : ITool
{
    
    private readonly Db _db;
    private readonly HttpClient _ollamaClient;

    public SaveMemoryTool(Db db, HttpClient http)
    {
        _db = db;
        _ollamaClient = http;
    }
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "save_memory",
            Description = "Saves durable memory. embedStr must be a clean, stable, factual sentence about the user.",
            Parameters = new()
            {
                Properties = new()
                {
                    ["memory"] = new()
                    {
                        Type = "string",
                        Description = "Final memory sentence to show to user"
                    },
                    ["type"] = new()
                    {
                        Type = "string",
                        Description =
                            "Type of memory. Fact/Skill/Preferences/Learning"
                    },
                },
                Required = new()
                {
                    "memory",
                    "type"
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        try
        {
            var userId = ctx.User.Id;
            if (!func.Arguments.TryGetValue("memory", out var memoryObj))
            {
                return "memory is missing";
            }
            var memory = memoryObj.ToString();
            var embedStr = await Utils.ExtractMemories(memory, _ollamaClient);
            if (!func.Arguments.TryGetValue("type", out var typeObj))
            {
                return "type is missing";
            }
            var type = typeObj;
                    
            await _db.SaveMemory(userId, memory, await Utils.GetEmbedding(embedStr, _ollamaClient), type.ToString());

            return "Successful!";
        }
        catch (Exception e)
        {
            Log.Error(e);
            return "Error: " + e.Message;
        }
    }
}