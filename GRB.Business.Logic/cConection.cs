using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRB.Business.Logic
{
    class cConection
    {
        public bool UpdateData(string Connection, string OfficeSpaceId, string DatabaseName, string TableName, NextwaverDB.NColumns NColumns, NextwaverDB.NWheres NWheres, string strDOC)
        {
            WorkSpace.Service WS = new WorkSpace.Service();
            string[] msgOutput = WS.UpdateData(Connection, OfficeSpaceId, DatabaseName, TableName, NColumns.ExportString(), NWheres.ExportString(), strDOC, "Admin");
            if (msgOutput[0].ToUpper() == "OK") return true;
            else
            {
                return false;
            }
        }

        public bool UpdateData(string Connection, string OfficeSpaceId, string DatabaseName, string TableName, NextwaverDB.NColumns NColumns, NextwaverDB.NWheres NWheres)
        {
            WorkSpace.Service WS = new WorkSpace.Service();
            string[] msgOutput = WS.UpdateData(Connection, OfficeSpaceId, DatabaseName, TableName, NColumns.ExportString(), NWheres.ExportString(), "", "Admin");
            if (msgOutput[0].ToUpper() == "OK") return true;
            else
            {
                return false;
            }
        }
    }
}
