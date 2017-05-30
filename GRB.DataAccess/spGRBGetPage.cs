using System;
using System.Data;
using System.Data.SqlClient;
using GRB_Dat = GRB.DataAccess;

namespace GRB.DataAccess
{
    public class spGRBGetPage
    {
        private const string _Procedure_Name = "dbo.spGRBGetPage";
        
        public static DataTable ExecuteReader(Int32 BookNo, Int32 PageNo)
        {
            SqlParameter Param_BookNo = new SqlParameter("@BookNo", BookNo);
            SqlParameter Param_PageNo = new SqlParameter("@PageNo", PageNo);

            return (DataTable)GRB_Dat.DBExecute.ExecuteProcedureReader(_Procedure_Name, false, Param_BookNo, Param_PageNo);
        }
    }
}
