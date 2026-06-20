using System.Text;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class SendTextFileTool : ITool
{
    public ToolRequest Definition => new ToolRequest()
    {
        Function = new()
        {
            Name = "send_text_file",
            Description = "Send text file",
            Parameters = new()
            {
                Properties = new()
                {
                    ["content"] = new()
                    {
                        Type = "string",
                        Description = "File content"
                    },
                    ["filename"] = new()
                    {
                        Type = "string",
                        Description = "File name with extension"
                    },
                    ["message"] = new()
                    {
                        Type = "string",
                        Description = "Message content"
                    }
                },
                Required = new() { "content", "filename" }
            },
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        try
        {
            if (!func.Arguments.TryGetValue("content", out var content))
            {
                return "content is missing!";
            }

            if (!func.Arguments.TryGetValue("filename", out var filename))
            {
                return "filename is missing!";
            }

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content.ToString()));
            await ctx.Channel.SendFileAsync(
                stream,
                filename.ToString(),
                func.Arguments.GetValueOrDefault("message")?.ToString() ?? ""
            );
            return "Successful!";
        }
        catch (Exception e)
        {
            Log.Error(e);
            return "Error: " + e.Message;
        }
    }
}