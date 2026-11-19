using CT.Postojna.Models;
using MySql.Data.MySqlClient;

namespace CT.Postojna.Services
{
    public class PersonService(DbConnection database)
    {
        public static IEnumerable<Person> GeneratePersons() => new List<Person>
        {
            new() { Id = 1, FirstName = "John", LastName = "Jones", Bio = "A software developer from New York.", Nickname = "Johnny", Height = 180.5m, Weight = 75.0m, Age = 30, Citizenship = "Russia", Nationality = "German", Released = DateTime.Now.AddYears(-5) },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith", Bio = "An artist based in Paris.", Nickname = "Janie", Height = 165.0m, Weight = 60.0m, Age = 28, Citizenship = "Russia", Nationality = "French", Released = DateTime.Now.AddYears(-3) }
        };

        public async Task<List<Person>> GetAllAsync()
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("SELECT Id, CategoryId, FirstName, LastName, Bio, Discontinued, Nickname, Height, Weight, Age, Citizenship, Nationality, Released FROM Person", connection);
            await using var reader = await command.ExecuteReaderAsync();
            var people = new List<Person>();

            while (await reader.ReadAsync())
            {
                people.Add(ReadPerson(reader));
            }

            return people;
        }

        public async Task<Person?> GetByIdAsync(int id)
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("SELECT Id, CategoryId, FirstName, LastName, Bio, Discontinued, Nickname, Height, Weight, Age, Citizenship, Nationality, Released FROM Person WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? ReadPerson(reader) : null;
        }

        public async Task<int> CreateAsync(Person person)
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("INSERT INTO Person (CategoryId, FirstName, LastName, Bio, Discontinued, Nickname, Height, Weight, Age, Citizenship, Nationality, Released) VALUES (@CategoryId, @FirstName, @LastName, @Bio, @Discontinued, @Nickname, @Height, @Weight, @Age, @Citizenship, @Nationality, @Released)", connection);
            AddParameters(command, person);
            await command.ExecuteNonQueryAsync();
            await using var idCommand = new MySqlCommand("SELECT LAST_INSERT_ID()", connection);
            return Convert.ToInt32(await idCommand.ExecuteScalarAsync());
        }

        public async Task<bool> UpdateAsync(int id, Person person)
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("UPDATE Person SET CategoryId = @CategoryId, FirstName = @FirstName, LastName = @LastName, Bio = @Bio, Discontinued = @Discontinued, Nickname = @Nickname, Height = @Height, Weight = @Weight, Age = @Age, Citizenship = @Citizenship, Nationality = @Nationality, Released = @Released WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            AddParameters(command, person);
            if (await command.ExecuteNonQueryAsync() > 0)
            {
                return true;
            }

            await using var existsCommand = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM Person WHERE Id = @Id)", connection);
            existsCommand.Parameters.AddWithValue("@Id", id);
            return Convert.ToInt32(await existsCommand.ExecuteScalarAsync()) != 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("DELETE FROM Person WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            return await command.ExecuteNonQueryAsync() > 0;
        }

        private static void AddParameters(MySqlCommand command, Person person)
        {
            command.Parameters.AddWithValue("@CategoryId", person.CategoryId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FirstName", person.FirstName);
            command.Parameters.AddWithValue("@LastName", person.LastName);
            command.Parameters.AddWithValue("@Bio", person.Bio);
            command.Parameters.AddWithValue("@Discontinued", person.Discontinued);
            command.Parameters.AddWithValue("@Nickname", person.Nickname);
            command.Parameters.AddWithValue("@Height", person.Height ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Weight", person.Weight ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Age", person.Age);
            command.Parameters.AddWithValue("@Citizenship", person.Citizenship);
            command.Parameters.AddWithValue("@Nationality", person.Nationality);
            command.Parameters.AddWithValue("@Released", person.Released ?? (object)DBNull.Value);
        }

        private static Person ReadPerson(System.Data.Common.DbDataReader reader) => new()
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("CategoryId")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            Bio = reader.GetString(reader.GetOrdinal("Bio")),
            Discontinued = reader.GetBoolean(reader.GetOrdinal("Discontinued")),
            Nickname = reader.GetString(reader.GetOrdinal("Nickname")),
            Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? null : reader.GetDecimal(reader.GetOrdinal("Height")),
            Weight = reader.IsDBNull(reader.GetOrdinal("Weight")) ? null : reader.GetDecimal(reader.GetOrdinal("Weight")),
            Age = reader.GetInt32(reader.GetOrdinal("Age")),
            Citizenship = reader.GetString(reader.GetOrdinal("Citizenship")),
            Nationality = reader.GetString(reader.GetOrdinal("Nationality")),
            Released = reader.IsDBNull(reader.GetOrdinal("Released")) ? null : reader.GetDateTime(reader.GetOrdinal("Released"))
        };
    }
}
