using System.Net.Http.Json;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class MathTool : ITool
{
    private readonly HttpClient _httpClient;

    public MathTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "math",
            Description = "Executes mathematical computations using SageMath.",
            Parameters = new()
            {
                Properties = new()
                {
                    ["expression"] = new()
                    {
                        Type = "string",
                        Description = "A valid SageMath expression that will be executed directly."
                    }
                },
                Required = new() { "expression" }
            }
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        try
        {
            var expr = func.Arguments.First().Value?.ToString();
            var response = await _httpClient.PostAsJsonAsync("http://localhost:8000/calc", new { expr });
            var result = await response.Content.ReadAsStringAsync();
            Log.Info($"Math Server response: {result}");
            return result;
        }
        catch (Exception e)
        {
            Log.Error(e);
            return "Error: " + e.Message;
        }
    }
}