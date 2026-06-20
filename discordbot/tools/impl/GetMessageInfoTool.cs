using System.Text.Json;
using Discord;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class GetMessageInfoTool : ITool
{
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "get_message_info",
            Description = "Get message info.",
            Parameters = new()
            {
                Properties = new()
                {
                    ["channel_id"] = new()
                    {
                        Type = "string",
                        Description = "The channel id"
                    },
                    ["message_id"] = new()
                    {
                        Type = "string",
                        Description = "The message id"
                    }
                },
                Required = new() { "channel_id", "message_id" }
            }
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        if (!func.Arguments.TryGetValue("channel_id", out var channelIdObj))
            return "channel id is missing";

        if (!func.Arguments.TryGetValue("message_id", out var messageIdObj))
            return "message id is missing";

        if (!ulong.TryParse(channelIdObj.ToString(), out var channelId))
            return "failed to parse channel id";

        if (!ulong.TryParse(messageIdObj.ToString(), out var messageId))
            return "failed to parse message id";

        var channel = ctx.Client.GetChannel(channelId);
        if (channel == null)
            return "Channel not found";

        if (channel is not ITextChannel textChannel)
            return "Channel is not ITextChannel";

        var message = await textChannel.GetMessageAsync(messageId);
        if (message == null)
            return "Message not found";

        var messageData = new
        {
            id = message.Id,
            content = message.Content,
            author = new
            {
                id = message.Author.Id,
                username = message.Author.Username,
                globalName = message.Author.GlobalName,
                isBot = message.Author.IsBot,
                avatarUrl = message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl()
            },
            channel = new
            {
                id = message.Channel.Id,
                name = (message.Channel as ITextChannel)?.Name
            },
            guild = (message.Channel as IGuildChannel) != null
                ? new { id = ((IGuildChannel)message.Channel).GuildId }
                : null,
            attachments = message.Attachments.Select(a => new
            {
                filename = a.Filename,
                contentType = a.ContentType,
                size = a.Size,
                url = a.Url,
                mediaUrl = a.ProxyUrl,
                width = a.Width,
                height = a.Height
            }),
            reference = message.Reference != null
                ? new
                {
                    messageId = message.Reference.MessageId,
                    channelId = message.Reference.ChannelId,
                    guildId = message.Reference.GuildId
                }
                : null,
            source = new
            {
                source = message.Source.ToString()
            }
        };

        return JsonSerializer.Serialize(messageData);
    }
}