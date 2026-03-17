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

        async public Task<User> InsertUser(UserCreateSchema user)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            INSERT INTO Users (Username, Email)
            VALUES ($username, $email)
            RETURNING UserID, Username, Email
            ";
            command.Parameters.AddWithValue("$username", user.Username);
            command.Parameters.AddWithValue("$email", user.Email);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2)
                };
            }
            throw new InvalidOperationException("Failed to insert user.");  // Handle edge cases (e.g., constraint violations)
        }

        async public Task<List<User>> GetAll(string? whereClause = null, params object[] parameters)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT UserID, Username, Email FROM Users";
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
                    Email = reader.GetString(2)
                });
            }
            return users;
        }

        async public Task<User?> GetOne(int userId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT UserID, Username, Email FROM Users WHERE UserID = $userId";
            command.Parameters.AddWithValue("$userId", userId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2)
                };
            }
            return null;
        }
    }
}