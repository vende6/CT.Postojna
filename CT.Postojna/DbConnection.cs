using MySql.Data.MySqlClient;

namespace CT.Postojna
{
    public class DbConnection
    {
        private readonly string connStr = "Server=YOUR_HOST;Port=3306;Database=YOUR_DB;Uid=YOUR_USER;Pwd=YOUR_PASSWORD;";

        public MySqlConnection CreateConnection() => new(connStr);
    }
}
