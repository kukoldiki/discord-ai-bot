using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using discordbot.models;
using HtmlAgilityPack;

namespace discordbot;

public class Utils
{
    public static readonly int MaxMessageLength = 1800;
    public static List<string> SplitByLength(string text, int maxLength)
    {
        var result = new List<string>();

        for (int i = 0; i < text.Length; i += maxLength)
        {
            result.Add(text.Substring(i, Math.Min(maxLength, text.Length - i)));
        }

        return result;
    }

    public static string GetTextFromHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        doc.DocumentNode.SelectNodes("//script|//style")?
            .ToList()
            .ForEach(n => n.Remove());
        return Regex.Replace(doc.DocumentNode.InnerText, @"\s+", " ").Trim();
    }
    
    public static string ReplaceChannels(string content, IReadOnlyCollection<SocketTextChannel> channels)
    {
        foreach (var channel in channels)
        {
            content = content.Replace(
                $"<#{channel.Id}>",
                $"#{channel.Name}"
            );
        }

        return content;
    }
    
    public static async Task<float[]> GetEmbedding(string input, HttpClient _ollamaClient)
    {
        var response = await _ollamaClient.PostAsJsonAsync("/api/embed", new
        {
            model = "nomic-embed-text",
            input
        });
        var obj = await response.Content.ReadFromJsonAsync<EmbeddingApiResponse>();
        return obj.Embeddings[0];
    }

    public static async Task<string> ExtractMemories(string message, HttpClient _ollamaClient)
    {
        var messages = new List<object> {
            new
            {
                role = "system",
                content = "You are a specialised text‑summarisation module. Your sole task is to compress any\nuser input into one short, neutral statement that captures the core meaning.\nThe statement will later be turned into an embedding and stored in the model’s\nmemory.\n\n### Output rules\n1. **Format** – a single line, no quotes, beginning with the word «Пользователь».\n2. **Length** – ≤ 15 words (ideally 8‑12).\n3. **Tone** – completely neutral, no emotions, no personal pronouns other than\n   «Пользователь».\n4. **Content** – keep the main subject, action, interest, goal or state.\n   Discard examples, jokes, emojis, filler words, dates, names of people (unless\n   they are the central topic) and any detail that does not affect the gist.\n5. **Generalisation** – if the utterance contains several tightly‑related ideas,\n   merge them into one concise summary without losing the primary intent.\n6. **Language** – Russian.\n7. **Empty / nonsense input** – output exactly:\n   Пользователь не предоставил значимой информации.\n\n### Processing steps\n1. Read the whole message.\n2. Identify the central predicate (what the user *does*, *feels*, *needs*,\n   *knows*, *wants*).\n3. Strip away introductory phrases, adverbs, emoticons, examples, dates,\n   and any non‑essential modifiers.\n4. Re‑phrase the core idea as “Пользователь …”.\n5. Verify that the result obeys the length and neutrality constraints."
            },
            new
            {
                role = "user",
                content = message
            }
        };
        var response = await _ollamaClient.PostAsJsonAsync("/api/chat",
        new {
            model = "qwen3:8b",
            think = false,
            stream = false,
            messages
        });
        return (await response.Content.ReadFromJsonAsync<ChatApiResponse>()).Message.Content;
    }
}