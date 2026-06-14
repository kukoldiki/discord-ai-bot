using System.Text.Json.Serialization;

namespace discordbot.models;

public class SearchResponse
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = "";

    [JsonPropertyName("results")]
    public List<SearchResult> Results { get; set; } = [];
}

public class SearchResult
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}