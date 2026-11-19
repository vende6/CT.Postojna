using CT.Postojna.Models;
using MySql.Data.MySqlClient;

namespace CT.Postojna.Services
{
    public class ActivityService(DbConnection database)
    {
        public static IEnumerable<Activity> GenerateActivities() => Enumerable.Range(1, 100).Select(i => new Activity
        {
            Id = i,
            CategoryId = i % 5 == 0 ? null : i % 5,
            CategoryName = i % 5 == 0 ? string.Empty : $"Category {i % 5}",
            Name = $"Activity {i}",
            Description = $"Description for activity {i}",
            Duration = $"{Random.Shared.Next(30, 120)} minutes",
            AheadOfTimeGOAT = string.Empty,
            values = Array.Empty<Person>()
        });

        public async Task<List<Activity>> GetAllAsync()
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("SELECT Id, CategoryId, CategoryName, Description, Name, Duration, AheadOfTimeGOAT FROM Activity", connection);
            await using var reader = await command.ExecuteReaderAsync();
            var activities = new List<Activity>();

            while (await reader.ReadAsync())
            {
                activities.Add(ReadActivity(reader));
            }

            return activities;
        }

        public async Task<Activity?> GetByIdAsync(int id)
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("SELECT Id, CategoryId, CategoryName, Description, Name, Duration, AheadOfTimeGOAT FROM Activity WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? ReadActivity(reader) : null;
        }

        public async Task<int> CreateAsync(Activity activity)
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("INSERT INTO Activity (CategoryId, CategoryName, Description, Name, Duration, AheadOfTimeGOAT) VALUES (@CategoryId, @CategoryName, @Description, @Name, @Duration, @AheadOfTimeGOAT)", connection);
            command.Parameters.AddWithValue("@CategoryId", activity.CategoryId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CategoryName", activity.CategoryName);
            command.Parameters.AddWithValue("@Description", activity.Description);
            command.Parameters.AddWithValue("@Name", activity.Name);
            command.Parameters.AddWithValue("@Duration", activity.Duration);
            command.Parameters.AddWithValue("@AheadOfTimeGOAT", activity.AheadOfTimeGOAT);
            await command.ExecuteNonQueryAsync();
            await using var idCommand = new MySqlCommand("SELECT LAST_INSERT_ID()", connection);
            return Convert.ToInt32(await idCommand.ExecuteScalarAsync());
        }

        public async Task<bool> UpdateAsync(int id, Activity activity)
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("UPDATE Activity SET CategoryId = @CategoryId, CategoryName = @CategoryName, Description = @Description, Name = @Name, Duration = @Duration, AheadOfTimeGOAT = @AheadOfTimeGOAT WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@CategoryId", activity.CategoryId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CategoryName", activity.CategoryName);
            command.Parameters.AddWithValue("@Description", activity.Description);
            command.Parameters.AddWithValue("@Name", activity.Name);
            command.Parameters.AddWithValue("@Duration", activity.Duration);
            command.Parameters.AddWithValue("@AheadOfTimeGOAT", activity.AheadOfTimeGOAT);
            if (await command.ExecuteNonQueryAsync() > 0)
            {
                return true;
            }

            await using var existsCommand = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM Activity WHERE Id = @Id)", connection);
            existsCommand.Parameters.AddWithValue("@Id", id);
            return Convert.ToInt32(await existsCommand.ExecuteScalarAsync()) != 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("DELETE FROM Activity WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            return await command.ExecuteNonQueryAsync() > 0;
        }

        private static Activity ReadActivity(System.Data.Common.DbDataReader reader) => new()
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("CategoryId")),
            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
            Description = reader.GetString(reader.GetOrdinal("Description")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Duration = reader.GetString(reader.GetOrdinal("Duration")),
            AheadOfTimeGOAT = reader.GetString(reader.GetOrdinal("AheadOfTimeGOAT")),
            values = Array.Empty<Person>()
        };
    }
}
