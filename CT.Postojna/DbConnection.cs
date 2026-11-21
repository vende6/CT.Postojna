using MySql.Data.MySqlClient;

namespace CT.Postojna
{
    public class DbConnection(IConfiguration configuration)
    {
        private readonly string connStr = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        public MySqlConnection CreateConnection() => new(connStr);
    }
}
