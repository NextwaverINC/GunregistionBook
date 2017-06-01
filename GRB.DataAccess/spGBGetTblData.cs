using System;
using System.Data;
using System.Data.SqlClient;
using GRB_Dat = GRB.DataAccess;

namespace GRB.DataAccess
{
    public class spGBGetTblData
    {
        private const string _Procedure_Name = "dbo.spGBGetTblData";

        public static DataTable ExecuteReader(string DateBegin, string DateEnd, string pStatus)
        {
            SqlParameter Param_DateBegin = new SqlParameter("@DateBegin", DateBegin);
            SqlParameter Param_DateEnd = new SqlParameter("@DateEnd", DateEnd);
            SqlParameter Param_Status = new SqlParameter("@Status", pStatus);

            return (DataTable)GRB_Dat.DBExecute.ExecuteProcedureReader(_Procedure_Name, false, Param_DateBegin, Param_DateEnd, Param_Status);
        }
    }
}
