using System.Data.SqlClient;
namespace Rastreo.DataAccess
{
    public static class ConnectionManager
    {
        public static SqlConnection CreateNewConnection()
        {
            return new SqlConnection(new DataAccessElement().ConnectionString.ConnectionString);
        }
    }
}
