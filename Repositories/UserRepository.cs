using Microsoft.Data.Sqlite;
using DotNetTutorial.Models;
using DotNetTutorial.Validation;

namespace DotNetTutorial.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        async public Task<User> InsertUser(User user)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            INSERT INTO Users (Username, Email, PasswordHash, Role)
            VALUES ($username, $email, $passwordHash, $role)
            RETURNING UserID, Username, Email, PasswordHash, Role
            ";
            command.Parameters.AddWithValue("$username", user.Username);
            command.Parameters.AddWithValue("$email", user.Email);
            command.Parameters.AddWithValue("$passwordHash", user.PasswordHash);
            command.Parameters.AddWithValue("$role", user.Role);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    Role = reader.GetString(4)
                };
            }
            throw new InvalidOperationException("Failed to insert user.");
        }

        async public Task<List<User>> GetAll(string? whereClause = null, params object[] parameters)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT UserID, Username, Email, Role FROM Users";
            if (!string.IsNullOrEmpty(whereClause))
            {
                command.CommandText += " WHERE " + whereClause;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue($"$param{i}", parameters[i]);
            }

            var users = new List<User>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    UserID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    Role = reader.GetString(3)
                });
            }
            return users;
        }

        async public Task<User?> GetById(int userId, bool includePassword = false)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT UserID, Username, Email, PasswordHash, Role FROM Users WHERE UserID = $userId";
            command.Parameters.AddWithValue("$userId", userId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = includePassword ? reader.GetString(3) : string.Empty,
                    Role = reader.GetString(4)
                };
            }
            return null;
        }

        async public Task<User?> GetByUsername(string username)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT UserID, Username, Email, PasswordHash, Role FROM Users WHERE Username = $username";
            command.Parameters.AddWithValue("$username", username);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3),
                    Role = reader.GetString(4)
                };
            }
            return null;
        }

        async public Task<User?> UpdateUser(int userId, UserUpdateSchema userUpdate)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();

            var setClauses = new List<string>();
            if (userUpdate.Username != null)
            {
                setClauses.Add("Username = $username");
                command.Parameters.AddWithValue("$username", userUpdate.Username);
            }
            if (userUpdate.Email != null)
            {
                setClauses.Add("Email = $email");
                command.Parameters.AddWithValue("$email", userUpdate.Email);
            }

            if (setClauses.Count == 0) return await GetById(userId);

            command.CommandText = $"UPDATE Users SET {string.Join(", ", setClauses)} WHERE UserID = $userId RETURNING UserID, Username, Email, PasswordHash, Role";
            command.Parameters.AddWithValue("$userId", userId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = string.Empty, // Don't return password hash
                    Role = reader.GetString(4)
                };
            }
            return null;
        }

        async public Task<bool> DeleteUser(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Users WHERE UserID = $userId";
            command.Parameters.AddWithValue("$userId", userId);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}