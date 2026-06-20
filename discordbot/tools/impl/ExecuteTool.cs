using System.Net.Http.Json;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class ExecuteTool : ITool
{
    
    private readonly HttpClient _ollamaClient;
    private readonly CommandConfig _config;

    public ExecuteTool(HttpClient httpClient, CommandConfig config)
    {
        _ollamaClient = httpClient;
        _config = config;
    }
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "execute",
            Description =
                "Executes a single system command and returns the result after completion. Linux. THIS IS SHELL.",
            Parameters = new()
            {
                Properties = new()
                {
                    ["command"] = new()
                    {
                        Type = "string",
                        Description = "The command to execute."
                    }
                },
                Required = new() { "command" }
            }
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        try
        {
            var execQuery = func.Arguments.First().Value?.ToString();
            Log.Info("AI executing " + execQuery);
            var execResponse = await _ollamaClient.PostAsJsonAsync($"{_config.ExecServerBaseUrl}/run", new
            {
                command = execQuery,
            });
            // var execApiResponse = await execResponse.Content.ReadFromJsonAsync<ExecApiResponse>();
            return await execResponse.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Log.Error(e);
            return "Error: " + e.Message ;
        }
    }
}