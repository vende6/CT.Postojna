using MySql.Data.MySqlClient;

namespace CT.Postojna
{
    public class DbConnection
    {
        private readonly string connStr = "Server=sql8.freesqldatabase.com;Port=3306;Database=sql8841003;Uid=sql8841003;Pwd=EZTwry86LA;";

        public MySqlConnection CreateConnection() => new(connStr);
    }
}
