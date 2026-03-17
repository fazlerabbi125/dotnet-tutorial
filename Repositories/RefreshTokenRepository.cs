using Microsoft.Data.Sqlite;
using DotNetTutorial.Models;

namespace DotNetTutorial.Repositories
{
    public class RefreshTokenRepository
    {
        private readonly string _connectionString;

        public RefreshTokenRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<RefreshToken> Create(int userId, string token, DateTime expiryTime)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = """
            INSERT INTO RefreshTokens (UserID, Token, ExpiryTime, IsRevoked)
            VALUES ($userId, $token, $expiryTime, 0)
            RETURNING Id, UserID, Token, ExpiryTime, IsRevoked
            """;
            cmd.Parameters.AddWithValue("$userId", userId);
            cmd.Parameters.AddWithValue("$token", token);
            cmd.Parameters.AddWithValue("$expiryTime", expiryTime.ToString("o"));

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RefreshToken
                {
                    Id = reader.GetInt32(0),
                    UserID = reader.GetInt32(1),
                    Token = reader.GetString(2),
                    ExpiryTime = DateTime.Parse(reader.GetString(3)),
                    IsRevoked = reader.GetInt32(4) == 1
                };
            }
            throw new InvalidOperationException("Failed to create refresh token.");
        }

        public async Task<RefreshToken?> GetByToken(string token)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, UserID, Token, ExpiryTime, IsRevoked FROM RefreshTokens WHERE Token = $token";
            cmd.Parameters.AddWithValue("$token", token);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RefreshToken
                {
                    Id = reader.GetInt32(0),
                    UserID = reader.GetInt32(1),
                    Token = reader.GetString(2),
                    ExpiryTime = DateTime.Parse(reader.GetString(3)),
                    IsRevoked = reader.GetInt32(4) == 1
                };
            }
            return null;
        }

        /// <summary>Revoke a specific token (e.g., on logout or rotation).</summary>
        public async Task<bool> Revoke(string token)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE RefreshTokens SET IsRevoked = 1 WHERE Token = $token";
            cmd.Parameters.AddWithValue("$token", token);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        /// <summary>Revoke all tokens for a user (e.g., on password change).</summary>
        public async Task RevokeAll(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE RefreshTokens SET IsRevoked = 1 WHERE UserID = $userId";
            cmd.Parameters.AddWithValue("$userId", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>Delete all tokens for a user (cleanup on account deletion).</summary>
        public async Task DeleteAll(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM RefreshTokens WHERE UserID = $userId";
            cmd.Parameters.AddWithValue("$userId", userId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
