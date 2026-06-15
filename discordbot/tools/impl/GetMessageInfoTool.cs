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
            cleanContent = message.CleanContent,
            timestamp = message.Timestamp.UtcDateTime,
            editedTimestamp = message.EditedTimestamp?.UtcDateTime,
            author = new
            {
                id = message.Author.Id,
                username = message.Author.Username,
                globalName = message.Author.GlobalName,
                discriminator = message.Author.Discriminator,
                isBot = message.Author.IsBot,
                isWebhook = message.Author.IsWebhook,
                mention = message.Author.Mention,
                avatarUrl = message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl()
            },
            channel = new
            {
                id = message.Channel.Id,
                name = (message.Channel as ITextChannel)?.Name,
                type = message.Channel.GetType().Name
            },
            guild = (message.Channel as IGuildChannel) != null
                ? new { id = ((IGuildChannel)message.Channel).GuildId }
                : null,
            mentions = new
            {
                users = message.MentionedUserIds.Select(x => new { id = x }),
                roles = message.MentionedRoleIds,
                channels = message.MentionedChannelIds
            },
            attachments = message.Attachments.Select(a => new
            {
                id = a.Id,
                filename = a.Filename,
                description = a.Description,
                contentType = a.ContentType,
                size = a.Size,
                url = a.Url,
                proxyUrl = a.ProxyUrl,
                width = a.Width,
                height = a.Height
            }),
            embeds = message.Embeds.Select(e => new
            {
                title = e.Title,
                description = e.Description,
                url = e.Url,
                type = e.Type.ToString()
            }),
            stickers = message.Stickers.Select(s => new { id = s.Id, name = s.Name }),
            reference = message.Reference != null
                ? new
                {
                    messageId = message.Reference.MessageId,
                    channelId = message.Reference.ChannelId,
                    guildId = message.Reference.GuildId
                }
                : null,
            flags = message.Flags?.ToString(),
            source = new
            {
                isTts = message.IsTTS,
                source = message.Source.ToString()
            }
        };

        return JsonSerializer.Serialize(messageData);
    }
}