using System.Text.Json;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class GetMemoryTool : ITool
{

    private readonly Db _db;
    private readonly HttpClient _ollamaClient;

    public GetMemoryTool(Db db, HttpClient http)
    {
        _db = db;
        _ollamaClient = http;
    }
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "get_memory",
            Description = "Search memory using semantic query.",
            Parameters = new()
            {
                Properties = new()
                {
                    ["memory"] = new()
                    {
                        Type = "string",
                        Description = "Semantic query for embedding search"
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
            var embedStr = await Utils.ExtractMemories(memoryObj.ToString(), _ollamaClient);
            if (!func.Arguments.TryGetValue("type", out var typeObj))
            {
                return "type is missing";
            }
            var type = typeObj;
                    
            var memories = await _db.SearchMemory(userId, await Utils.GetEmbedding(embedStr, _ollamaClient), type.ToString());
            if (memories == null)
            {
                return "memory not found";
            }

            return JsonSerializer.Serialize(memories);
        }
        catch (Exception e)
        {
            Log.Error(e);
            return "Error: " + e.Message;
        }
    }
}