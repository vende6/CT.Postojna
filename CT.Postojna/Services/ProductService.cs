using CT.Postojna.Models;
using MySql.Data.MySqlClient;

namespace CT.Postojna.Services
{
    public class ProductService(DbConnection database)
    {
        public static IEnumerable<Product> GenerateProducts() => Enumerable.Range(1, 100).Select(i => new Product
        {
            Id = i,
            Name = $"Product {i}",
            Description = $"Description for product {i}",
            Price = Random.Shared.Next(1, 9999) * 1.23m,
            Quantity = Random.Shared.Next(1, 9999),
            Released = DateTime.Now.AddDays(-Random.Shared.Next(1, 3000)),
            Discontinued = i % 4 == 0,
            CategoryId = i % 5 == 0 ? null : i % 5,
            CategoryName = i % 5 == 0 ? string.Empty : $"Category {i % 5}"
        });

        public async Task<List<Product>> GetAllAsync()
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("SELECT Id, CategoryId, CategoryName, Description, Discontinued, Name, Price, Quantity, Released FROM Product", connection);
            await using var reader = await command.ExecuteReaderAsync();
            var products = new List<Product>();

            while (await reader.ReadAsync())
            {
                products.Add(ReadProduct(reader));
            }

            return products;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("SELECT Id, CategoryId, CategoryName, Description, Discontinued, Name, Price, Quantity, Released FROM Product WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? ReadProduct(reader) : null;
        }

        public async Task<int> CreateAsync(Product product)
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("INSERT INTO Product (CategoryId, CategoryName, Description, Discontinued, Name, Price, Quantity, Released) VALUES (@CategoryId, @CategoryName, @Description, @Discontinued, @Name, @Price, @Quantity, @Released)", connection);
            AddParameters(command, product);
            await command.ExecuteNonQueryAsync();
            await using var idCommand = new MySqlCommand("SELECT LAST_INSERT_ID()", connection);
            return Convert.ToInt32(await idCommand.ExecuteScalarAsync());
        }

        public async Task<bool> UpdateAsync(int id, Product product)
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("UPDATE Product SET CategoryId = @CategoryId, CategoryName = @CategoryName, Description = @Description, Discontinued = @Discontinued, Name = @Name, Price = @Price, Quantity = @Quantity, Released = @Released WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            AddParameters(command, product);
            if (await command.ExecuteNonQueryAsync() > 0)
            {
                return true;
            }

            await using var existsCommand = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM Product WHERE Id = @Id)", connection);
            existsCommand.Parameters.AddWithValue("@Id", id);
            return Convert.ToInt32(await existsCommand.ExecuteScalarAsync()) != 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            await using var connection = database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new MySqlCommand("DELETE FROM Product WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            return await command.ExecuteNonQueryAsync() > 0;
        }

        private static void AddParameters(MySqlCommand command, Product product)
        {
            command.Parameters.AddWithValue("@CategoryId", product.CategoryId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CategoryName", product.CategoryName);
            command.Parameters.AddWithValue("@Description", product.Description);
            command.Parameters.AddWithValue("@Discontinued", product.Discontinued);
            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Price", product.Price ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Quantity", product.Quantity);
            command.Parameters.AddWithValue("@Released", product.Released ?? (object)DBNull.Value);
        }

        private static Product ReadProduct(System.Data.Common.DbDataReader reader) => new()
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("CategoryId")),
            CategoryName = ReadString(reader, "CategoryName"),
            Description = ReadString(reader, "Description"),
            Discontinued = reader.GetBoolean(reader.GetOrdinal("Discontinued")),
            Name = ReadString(reader, "Name"),
            Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? null : reader.GetDecimal(reader.GetOrdinal("Price")),
            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
            Released = reader.IsDBNull(reader.GetOrdinal("Released")) ? null : reader.GetDateTime(reader.GetOrdinal("Released"))
        };

        private static string ReadString(System.Data.Common.DbDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }
    }
}
