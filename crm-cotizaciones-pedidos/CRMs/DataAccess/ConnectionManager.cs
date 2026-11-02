using System.Data.SqlClient;
namespace CRMs.DataAccess
{
    public static class ConnectionManager
    {
        public static SqlConnection CreateNewConnection()
        {
            return new SqlConnection(new DataAccessElement().ConnectionString.ConnectionString);
        }
    }
}
