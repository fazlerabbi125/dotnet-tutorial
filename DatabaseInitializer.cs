// DatabaseInitializer.cs
using Microsoft.Data.Sqlite;

public static class DatabaseInitializer
{
    public static void Initialize(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();

        // Users table (no refresh token columns)
        cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS Users(
            UserID INTEGER PRIMARY KEY AUTOINCREMENT,
            Username TEXT NOT NULL,
            Email TEXT NOT NULL,
            PasswordHash TEXT NOT NULL,
            Role TEXT NOT NULL DEFAULT 'User'
        );
        """;
        cmd.ExecuteNonQuery();

        // Separate RefreshTokens table
        cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS RefreshTokens(
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UserID INTEGER NOT NULL,
            Token TEXT NOT NULL UNIQUE,
            ExpiryTime TEXT NOT NULL,
            IsRevoked INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
        );
        """;
        cmd.ExecuteNonQuery();
    }
}
