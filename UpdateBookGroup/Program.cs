using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UpdateBookGroup
{
    class Program
    {
        static void Main(string[] args)
        {
            //string Connection = "NextwaverDatabase";
            //string OfficeSpaceId = "OF.0001";
            //string DatabaseName = "GunBook";
            //string UserName = "System";

            //String[] EditImageTemplate = System.IO.File.ReadAllLines("BookEdit.txt");

            //WorkSpace.Service WS = new WorkSpace.Service();

            //string BookID = "";
            //string GunGroup = "";
            //string Prefix = "";

            //DataTable dt = WS.SelectAll(Connection, OfficeSpaceId, DatabaseName, "Book", UserName);

            //try
            //{
            //    foreach (string item in EditImageTemplate)
            //    {
            //        BookID = getParam(item, "BOOKNO");
            //        GunGroup = getParam(item, "GROUPGUN");
            //        Prefix = getParam(item, "PREFIX");

            //        string strID = "";

            //        DataRow[] foundRows = dt.Select("BOOKNO = '" + BookID + "'");

            //        if (foundRows.Length > 0)
            //            strID = foundRows[0]["ID"].ToString();

            //        NextwaverDB.NColumns NCS = new NextwaverDB.NColumns();
            //        NCS.Add(new NextwaverDB.NColumn("GROUPGUN", GunGroup));
            //        NCS.Add(new NextwaverDB.NColumn("GUNREGIDPREFIX", Prefix));

            //        NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
            //        NWS.Add(new NextwaverDB.NWhere("BOOKNO", BookID));
            //        if (strID.Trim() != "")
            //            NWS.Add(new NextwaverDB.NWhere("ID", strID));

            //        string[] msgOutput = WS.UpdateData(Connection, OfficeSpaceId, DatabaseName, "Book", NCS.ExportString(), NWS.ExportString(), "", UserName);
            //        if (msgOutput[0].ToUpper() == "OK")
            //        {
            //            System.Console.WriteLine("Update -->> BookNo " + BookID);
            //        }
            //        else
            //        {
            //            File.AppendAllText("Log.txt", Environment.NewLine + Environment.NewLine
            //                          + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " :: Book:" + BookID + " --> " + msgOutput[1]);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    File.AppendAllText("Log.txt", Environment.NewLine + Environment.NewLine
            //                           + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " :: Book:" + BookID + " --> " + ex.Message);
            //    throw;
            //}
        }

        static void AddDataXmlNode(XmlDocument xmlDoc, string xPath, string sValue)
        {
            XmlNode nodeControl = xmlDoc.SelectSingleNode(xPath);
            nodeControl.Attributes["Value"].Value = sValue;
        }

        static string convertDatetime(DateTime date)
        {
            int yearTh = date.Year;
            if (yearTh < 2400)
                yearTh = yearTh + 543;

            return date.Day.ToString() + "/" + date.Month.ToString() + "/" + yearTh.ToString() + " " + date.Hour.ToString() + ":" + date.Minute.ToString() + ":" + date.Second.ToString();
        }

        static string getParam(string ParamList, string strParam)
        {
            string strResult = "";
            string[] listparam = ParamList.Split('&');
            foreach (var item in listparam)
            {
                if (item.Split('=')[0] == strParam)
                    strResult = item.Split('=')[1];
            }
            return strResult;
        }

    }
}
