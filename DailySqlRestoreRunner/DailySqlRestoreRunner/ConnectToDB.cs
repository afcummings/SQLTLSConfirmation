using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace DailySqlRestoreRunner
{
    class ConnectToDB
    {
        public static IDbConnection DefaultConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["serverConnection"].ConnectionString);
        }

        public static IDbConnection OtherConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["serverConnection2"].ConnectionString);
        }
    }
}
