using Dapper;
using discordbot.models;
using Npgsql;
using Pgvector;

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
            @"INSERT INTO user_settings (user_id, model, system_prompt, thinking, profile)
              VALUES (@UserId, @Model, @SystemPrompt, @Thinking, @Profile)",
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
          system_prompt = @SystemPrompt,
      thinking = @Thinking
      profile = @Profile
      WHERE user_id = @UserId",
            s
        );
    }
    
    public async Task SaveMemory(
        ulong userId,
        string content,
        float[] embedding,
        string type)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();

        var sql = @"
        INSERT INTO memories (user_id, content, embedding, type)
        VALUES (@user_id, @content, @embedding, @type);
    ";

        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("user_id", (long)userId);
        cmd.Parameters.AddWithValue("content", content);
        cmd.Parameters.AddWithValue("type", type);
        
        cmd.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "embedding",
            DataTypeName = "vector",
            Value = new Vector(embedding)
        });

        await cmd.ExecuteNonQueryAsync();
    }
    
    public async Task<List<Memory>> SearchMemory(
        ulong userId,
        float[] queryEmbedding,
        string type)
    {
        var result = new List<Memory>();

        await using var conn = CreateConnection();
        await conn.OpenAsync();

        var sql = @"
        SELECT id, content, created_at, type
        FROM memories
        WHERE user_id = @user_id AND type = @type
        ORDER BY embedding <=> @embedding
        LIMIT 5;
    ";

        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("user_id", (long)userId);
        cmd.Parameters.AddWithValue("type", type);
        cmd.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "embedding",
            DataTypeName = "vector",
            Value = new Vector(queryEmbedding)
        });

        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new Memory
            {
                Id = reader.GetInt64(0),
                Content = reader.GetString(1),
                CreatedAt = reader.GetDateTime(2)
            });
        }

        return result;
    }
}