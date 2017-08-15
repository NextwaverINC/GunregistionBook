using System;
using System.Data;
using System.Data.SqlClient;
using GRB_Dat = GRB.DataAccess; 

namespace GRB.DataAccess
{
    public class spGRBGetBookByCondition
    {
        private const string _Procedure_Name = "dbo.spGRBGetBookByCondition";

        public static DataTable ExecuteReader(Int32 BookNo, Int32 BookYear)
        {
            SqlParameter Param_BookNo = new SqlParameter("@BookNo", BookNo);
            SqlParameter Param_Year = new SqlParameter("@Year", BookYear);

            return (DataTable)GRB_Dat.DBExecute.ExecuteProcedureReader(_Procedure_Name, false, Param_BookNo, Param_Year);
        }
    }
}
