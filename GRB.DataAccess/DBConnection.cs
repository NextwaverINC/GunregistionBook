using System;
using System.Configuration;
using System.Data.SqlClient;

namespace GRB.DataAccess
{
    public class DBConnection
    {
        public static SqlConnection GetDBConnection()
        {
            //LIB_Cmn.LogControl.LogTracer("DBConnection.GetDBConnection");

            SqlConnection DbConnect = new SqlConnection();

            //DbConnect.ConnectionString = "Data Source=DESKTOP-LA9PNGG\\SQLEXPRESS;Initial Catalog=Gun_Registration;Persist Security Info=True;User ID=sa;Password=nextwaver;Connect Timeout=0";
            DbConnect.ConnectionString = ConfigurationManager.AppSettings["cfgDBConnect"];

            return DbConnect;
        }
    }
}
