using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class HttpGetTool : ITool
{
    private readonly HttpClient _ollamaClient;

    public HttpGetTool(HttpClient httpClient)
    {
        _ollamaClient = httpClient;
    }
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "http_get",
            Description =
                "Fetches content from a public HTTPS URL using HTTP GET.\nOnly HTTPS URLs allowed. Returns raw response body as text.\nUsed for reading web pages or API responses.",
            Parameters = new()
            {
                Properties = new()
                {
                    ["url"] = new PropertyDefinition()
                    {
                        Type = "string",
                        Description =
                            "A full absolute HTTPS URL to fetch via HTTP GET. Must not be localhost or private IP. Only HTTPS is allowed."
                    }
                },
                Required = new() { "url" }
            }
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        try
        {
            var url = func.Arguments.First().Value?.ToString();
            if (!url.Contains("https://"))
            {
                return "URL must contains https schema";
            }

            Log.Info("AI visting " + url);

            var response = await _ollamaClient.GetAsync("https://r.jina.ai/" + url);
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Log.Error(e);
            return "Error: " + e.Message;
        }
    }
}