using Discord;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class WriteDmTool : ITool
{
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "write_to_dm",
            Description = "Send a message to a Discord dms. " +
                          "Use this only when the user explicitly asks to send a message " +
                          "to another user or when a task requires posting information " +
                          "to a specific Discord user." +
                          "Dont use this for spam!",
            Parameters = new()
            {
                Properties = new()
                {
                    ["user_id"] = new()
                    {
                        Type = "string",
                        Description = "Target Discord user id."
                    },
                    ["message"] = new()
                    {
                        Type = "string",
                        Description = "The message content to send."
                    }
                },
                Required = new()
                {
                    "user_id",
                    "message"
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        // return "Tool disabled because you're spamming";
        try
        {
            if (!func.Arguments.TryGetValue("user_id", out var userIdObj))
            {
                return "user id is missing";
            }

            if (!ulong.TryParse(userIdObj.ToString(), out var userId))
            {
                Log.Error("Failed to parse user id " + func.Arguments.First());
                return "failed to parse used id";
            }

            if (!func.Arguments.TryGetValue("message", out var contentObj))
            {
                return "content is missing";
            }

            var user = ctx.Client.GetUser(userId);
            var content = contentObj.ToString();
            if (content.Length > Utils.MaxMessageLength)
                content = content.Substring(0, Utils.MaxMessageLength);
            await user.SendMessageAsync($"{content}\n\nMesssage sent by {ctx.User.Mention}");
            return "Successful!";
        }
        catch (Exception e)
        {
            return "Error: " + e.Message;
            Log.Error(e);
        }
    }
}