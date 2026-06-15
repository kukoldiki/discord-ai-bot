using System.Text;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class ReadMessagesTool : ITool
{
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "read_messages",
            Description = "Reads message history in specified channel",
            Parameters = new()
            {
                Properties = new()
                {
                    ["channel_name"] = new()
                    {
                        Type = "string",
                        Description = "The channel name where read message history"
                    },
                    ["message_count"] = new()
                    {
                        Type = "int",
                        Description = "Message count to fetch, max is 99"
                    }
                },
                Required = new() { "channel_name", "message_count" }
            }
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        try
        {
            if (!func.Arguments.TryGetValue("channel_name", out var channelNameObj))
            {
                return "channel_name is missing";
            }

            if (!func.Arguments.TryGetValue("message_count", out var messageCountStr))
            {
                return "read messages is missing";
            }

            if (!int.TryParse(messageCountStr.ToString(), out var messageCount))
            {
                return "Failed to parse message count";
            }

            if (messageCount > 99 || messageCount < 1)
            {
                return "Message count must be between 1 and 99";
            }

            var channelName = channelNameObj.ToString();
            var channel = ctx.Guild.TextChannels.FirstOrDefault(c => c.Name == channelName);
            if (channel == null)
            {
                return "channel not found";
            }

            var messages = channel.GetMessagesAsync(limit: messageCount);
            var sbMessages = new StringBuilder();
            await foreach (var batch in messages)
            {
                foreach (var message in batch)
                {
                    if (message.Author.IsBot) continue;
                    var content = $"[{message.Id}] {message.Author.Username}({message.Author.Id}): {message.Content}";
                    if (content.Length > 700)
                        content = content.Substring(0, 700);
                    sbMessages.AppendLine(content);
                }
            }
            
            return sbMessages.ToString();
        }
        catch (Exception e)
        {
            return "Error: " + e.Message;
        }
    }
}