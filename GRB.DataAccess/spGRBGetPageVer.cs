using System;
using System.Data;
using System.Data.SqlClient;
using GRB_Dat = GRB.DataAccess;

namespace GRB.DataAccess
{
    public class spGRBGetPageVer
    {
        private const string _Procedure_Name = "dbo.spGRBGetPageVer";
        
        public static DataTable ExecuteReader(Int32 BookNo, Int32 PageNo, Int32 PageVersion)
        {
            SqlParameter Param_BookNo = new SqlParameter("@BookNo", BookNo);
            SqlParameter Param_PageNo = new SqlParameter("@PageNo", PageNo);
            SqlParameter Param_PageVersion = new SqlParameter("@PageVersion", PageVersion);

            return (DataTable)GRB_Dat.DBExecute.ExecuteProcedureReader(_Procedure_Name, false, Param_BookNo, Param_PageNo, Param_PageVersion);
        }
    }
}
