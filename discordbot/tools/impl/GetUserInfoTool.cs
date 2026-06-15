using System.Text.Json;
using Discord.Commands;
using discordbot.models;

namespace discordbot.tools.impl;

public class GetUserInfoTool : ITool
{
    public ToolRequest Definition => new()
    {
        Function = new()
        {
            Name = "get_user_info",
            Description = "Get user info.",
            Parameters = new()
            {
                Properties = new()
                {
                    ["user_id"] = new()
                    {
                        Type = "string",
                        Description = "The user id"
                    }
                },
                Required = new() { "user_id" }
            }
        }
    };

    public Task<string> ExecuteAsync(ToolFunction func, SocketCommandContext ctx)
    {
        if (!ulong.TryParse(func.Arguments.First().Value?.ToString(), out var userId))
        {
            Log.Error("Failed to parse user id " + func.Arguments.First());
            return Task.FromResult("Failed to parse user id");
        }

        var user = ctx.Client.GetUser(userId);
        if (user == null)
            return Task.FromResult("User not found");

        var safeUser = new
        {
            id = user.Id,
            username = user.Username,
            globalName = user.GlobalName,
            discriminator = user.Discriminator,
            isBot = user.IsBot,
            avatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl(),
            createdAt = user.CreatedAt.UtcDateTime,
            user.Status,
            user.Mention,
            mutualGuildsCount = user.MutualGuilds?.Count ?? 0
        };

        return Task.FromResult(JsonSerializer.Serialize(safeUser));
    }
}