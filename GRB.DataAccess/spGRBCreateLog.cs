using System;
using System.Data;
using System.Data.SqlClient;
using GRB_Dat = GRB.DataAccess;


namespace GRB.DataAccess
{
    public class spGRBCreateLog
    {
        private const string _Procedure_Name = "dbo.spGRBCreateLog";

        public static DataTable ExecuteReader(string UserName, string Menu, string Process, string Result, string Param)
        {
            SqlParameter Param_UserName = new SqlParameter("@UserName", UserName);
            SqlParameter Param_Menu = new SqlParameter("@Menu", Menu);
            SqlParameter Param_Process = new SqlParameter("@Process", Process);
           
            SqlParameter Param_Result = new SqlParameter("@Result", Result);
            SqlParameter Param_Param = new SqlParameter("@Param", Param);

            return (DataTable)GRB_Dat.DBExecute.ExecuteProcedureReader(_Procedure_Name, false, Param_UserName, Param_Menu, Param_Process, Param_Result, Param_Param);
        }
    }   
}
