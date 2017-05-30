using System;
using System.Data;
using System.Data.SqlClient;
using GRB_Dat = GRB.DataAccess; 

namespace GRB.DataAccess
{
    public class spGRBGetBook
    {
        private const string _Procedure_Name = "dbo.spGRBGetBook";

        public static DataTable ExecuteReader(Int32 BookNo)
        {
            SqlParameter Param_BookNo = new SqlParameter("@BookNo", BookNo);

            return (DataTable)GRB_Dat.DBExecute.ExecuteProcedureReader(_Procedure_Name, false, Param_BookNo);
        }
    }
}
