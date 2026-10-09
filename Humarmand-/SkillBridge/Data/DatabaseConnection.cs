using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SkillBridge.Data
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SkillBridgeDB") 
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=SkillBridgeDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public SqlConnection GetConnection() => new SqlConnection(_connectionString);
    }
}
