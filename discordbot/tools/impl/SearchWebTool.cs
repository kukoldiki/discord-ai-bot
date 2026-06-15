using System.Net.Http.Json;
using System.Text;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class SearchWebTool : ITool
{
    
    private readonly HttpClient _ollamaClient;
    private readonly CommandConfig _config;

    public SearchWebTool(HttpClient httpClient, CommandConfig config)
    {
        _ollamaClient = httpClient;
        _config = config;
    }
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "search_web",
            Description = "Search the web for information and return a list of relevant search results.",
            Parameters = new()
            {
                Properties = new()
                {
                    ["query"] = new()
                    {
                        Type = "string",
                        Description =
                            "The search query to send to the search engine. Use concise and specific keywords that best describe the information you need to find."
                    }
                },
                Required = new() { "query" }
            }
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        var q = func.Arguments.First().Value?.ToString();
        var resp = await _ollamaClient.GetAsync($"{_config.searxngAddress}/search?q={q}&format=json&limit=3");
        var obj = await resp.Content.ReadFromJsonAsync<SearchResponse>();
        var sb = new StringBuilder();
        foreach (var res in obj.Results)
        {
            sb.AppendLine($"Title: {res.Title}");
            sb.AppendLine($"Snippet: {res.Content}");
            sb.AppendLine($"URL: {res.Url}");
            sb.AppendLine("");
        }
        
        // await ReplyAsync($"Fond {obj.Results.Count} results");
        return sb.ToString();
    }
}