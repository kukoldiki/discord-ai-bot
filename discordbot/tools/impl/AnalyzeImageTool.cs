using System.Net.Http.Json;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class AnalyzeImageTool : ITool
{
    private readonly HttpClient _httpClient;

    public AnalyzeImageTool(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "analyze_image",
            Description = "Analyze image using gemma4:31b.",
            Parameters = new()
            {
                Properties = new()
                {
                    ["image_url"] = new()
                    {
                        Type = "string",
                        Description = "A valid https link to image"
                    },
                    ["prompt"] = new()
                    {
                        Type = "string",
                        Description = "The prompt for gemma4."
                    }
                },
                Required = new() { "image_url" }
            }
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        try
        {
            if (!func.Arguments.TryGetValue("image_url", out var imageUrlObj))
                return "image url is missing";

            var prompt = func.Arguments.GetValueOrDefault("prompt")?.ToString()
                         ?? "Что ты видишь на данной картинке? Кратко опиши ее, если есть текст - зачитай.";

            byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrlObj.ToString());
            var image64 = Convert.ToBase64String(imageBytes);

            var messages = new List<object>
            {
                new { role = "system", content = prompt },
                new { Role = "user", Content = "", images = new[] { image64 } }
            };

            var response = await _httpClient.PostAsJsonAsync("/api/chat", new
            {
                messages,
                stream = false,
                model = "gemma4:31b-cloud"
            });

            var chatResponse = await response.Content.ReadFromJsonAsync<ChatApiResponse>();
            return chatResponse?.Message.Content ?? "No response";
        }
        catch (Exception e)
        {
            Log.Error(e);
            return "Error: " + e.Message;
        }
    }
}