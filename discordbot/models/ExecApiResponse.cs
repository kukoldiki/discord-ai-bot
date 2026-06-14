using System.Text.Json.Serialization;

namespace discordbot.models;

public class ExecApiResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("exitCode")]
    public int ExitCode { get; set; }

    [JsonPropertyName("timeout")]
    public bool Timeout { get; set; }

    [JsonPropertyName("blocked")]
    public bool Blocked { get; set; }

    [JsonPropertyName("output")]
    public string Output { get; set; }
}