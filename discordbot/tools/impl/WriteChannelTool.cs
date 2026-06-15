using Discord;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class WriteChannelTool : ITool
{
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "write_to_channel",
            Description = "Send a message to a Discord channel. " +
                          "Use this only when the user explicitly asks to send a message " +
                          "to another channel or when a task requires posting information " +
                          "to a specific Discord channel.",
            Parameters = new()
            {
                Properties = new()
                {
                    ["channel_name"] = new()
                    {
                        Type = "string",
                        Description = "Target Discord channel name."
                    },
                    ["message"] = new()
                    {
                        Type = "string",
                        Description = "The message content to send."
                    }
                },
                Required = new()
                {
                    "channel_name",
                    "message"
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        try
        {
            if (!func.Arguments.TryGetValue("channel_name", out var name))
            {
                Log.Info("Failed to get channel_id");
                return "channel_name is missing!";
            }

            if (!func.Arguments.TryGetValue("message", out var contentObj))
            {
                Log.Info("Failed to get message");
                return "message is missing!";
            }

            Log.Info($"{name} : {contentObj}");

            var channel = ctx.Guild.TextChannels.FirstOrDefault(c => c.Name == name.ToString());
            if (channel != null && channel is ITextChannel textChannel)
            {
                var content = contentObj.ToString();
                if (content.Length > Utils.MaxMessageLength)
                {
                    var parts = Utils.SplitByLength(content, Utils.MaxMessageLength);
                    IThreadChannel? thread = null;
                    foreach (var part in parts)
                    {
                        if (thread == null)
                        {
                            var message = await textChannel.SendMessageAsync(part);
                            thread = await textChannel.CreateThreadAsync(name: "Message", message: message);
                        }
                        else
                        {
                            await thread.SendMessageAsync(part);
                        }
                    }
                }
                else
                {
                    await channel.SendMessageAsync(content);
                }

                return "message sent!";
            }
            else
            {
                Log.Info("channel check failed");
                return "channel is not ITextChannel";
            }
        }
        catch (Exception e)
        {
            return "Failed write to channel! " + e.Message;
            Log.Error(e);
        }
    }
}