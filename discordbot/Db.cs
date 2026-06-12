using Dapper;
using discordbot.models;
using Npgsql;

namespace discordbot;

public class Db
{
    private readonly string _connectionString;

    public Db(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection CreateConnection()
        => new NpgsqlConnection(_connectionString);
    
    public async Task<UserSettings> GetOrCreateUserSettings(long userId)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();

        var settings = await conn.QueryFirstOrDefaultAsync<UserSettings>(
            "SELECT * FROM user_settings WHERE user_id = @userId",
            new { userId }
        );

        if (settings != null)
            return settings;

        settings = new UserSettings
        {
            UserId = userId
        };

        await conn.ExecuteAsync(
            @"INSERT INTO user_settings (user_id, model, system_prompt)
              VALUES (@UserId, @Model, @SystemPrompt)",
            settings
        );

        return settings;
    }

    public async Task UpdateUserSettings(UserSettings s)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();

        var rows = await conn.ExecuteAsync(
            @"UPDATE user_settings
      SET model = @Model,
          system_prompt = @SystemPrompt
      WHERE user_id = @UserId",
            s
        );
    }
}