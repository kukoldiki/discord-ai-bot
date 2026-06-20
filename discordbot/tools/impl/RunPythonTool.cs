using System.Net.Http.Json;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class RunPythonTool : ITool
{
    private readonly HttpClient _ollamaClient;
    private readonly CommandConfig _config;

    public RunPythonTool(HttpClient httpClient, CommandConfig config)
    {
        _ollamaClient = httpClient;
        _config = config;
    }
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "run_python",
            Description = "Executes a python scripts.",
            Parameters = new()
            {
                Properties = new()
                {
                    ["script"] = new()
                    {
                        Type = "string",
                        Description = "The script to execute."
                    }
                },
                Required = new() { "script" }
            }
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        try
        {
            var pythonCode = func.Arguments.First().Value?.ToString();
            var execResponse = await _ollamaClient.PostAsJsonAsync($"{_config.ExecServerBaseUrl}/python", new
            {
                code = pythonCode,
            });
            // var execApiResponse = await execResponse.Content.ReadFromJsonAsync<ExecApiResponse>();

            return await execResponse.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Log.Error(e);
            return "Error: "+e.Message;
        }
    }
}