using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Hunarmand.Data
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("HunarmandDB") 
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=HunarmandDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public SqlConnection GetConnection() => new SqlConnection(_connectionString);
    }
}
