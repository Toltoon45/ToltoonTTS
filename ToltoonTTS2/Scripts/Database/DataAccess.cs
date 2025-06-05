
using Microsoft.Data.Sqlite;
using System.Configuration;

namespace ToltoonTTS2.Scripts.Database
{
    internal class DataAccess
    {
        string _connectionString;

        public DataAccess(string dbName)
        {
            _connectionString = ConfigurationManager.ConnectionStrings[dbName].ConnectionString;
        }
        public void TestConnection()
        {
            try
            {
                var con = new SqliteConnection(_connectionString);
                con.Open();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
