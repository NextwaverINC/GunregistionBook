using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GRB.Console.Destrory
{
    class Destrory
    {
        string Connection = "NextwaverDatabase";
        string OfficeSpaceId = "OF.0001";
        string DatabaseName = "GunBook";
        XmlDocument xDoc;
        void AddDataXmlNode(string xPath, string sValue)
        {
            XmlNode nodeControl = xDoc.SelectSingleNode(xPath);
            nodeControl.Attributes["Value"].Value = sValue;
        }
        string convertDatetime(DateTime date)
        {
            int yearTh = date.Year;
            if (yearTh < 2400)
                yearTh = yearTh + 543;

            return date.Day.ToString() + "/" + date.Month.ToString() + "/" + yearTh.ToString() + " " + date.Hour.ToString() + ":" + date.Minute.ToString() + ":" + date.Second.ToString();
        }
        public void Start()
        {
            
            GRB_WebService.GRB_WebService GRBS = new GRB_WebService.GRB_WebService();
            DataTable dt = GRBS.getDestroryBook();
            WorkSpaceService.Service WS = new WorkSpaceService.Service();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string ID = "" + dt.Rows[i]["ID"];

                String strDoc = WS.SelectLastDocument(Connection, OfficeSpaceId, DatabaseName, "Book", int.Parse(ID), "system");
                xDoc = new XmlDocument();
                xDoc.LoadXml(strDoc);//Recycle

                string RootPathBook = "//Document/Data/Section[@ID='1']/Items[@Name='GunBook']";
                AddDataXmlNode(RootPathBook + "/Item[@Name='BookStatus']", "Destrory");
                AddDataXmlNode(RootPathBook + "/Item[@Name='UpdateDate']", convertDatetime(DateTime.Now));
                AddDataXmlNode(RootPathBook + "/Item[@Name='UpdateBy']", "system");


                NextwaverDB.NColumns NCS = new NextwaverDB.NColumns();
                NCS.Add(new NextwaverDB.NColumn("BOOKSTATUS", "Destrory"));
                NCS.Add(new NextwaverDB.NColumn("UPDATEDATE", DateTime.Now.ToString("dd/MM/yyyy")));
                NCS.Add(new NextwaverDB.NColumn("UPDATEBY", "system"));

                NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
                NWS.Add("ID", ID);
                strDoc = xDoc.OuterXml;
                string[] OP = WS.UpdateData(Connection, OfficeSpaceId, DatabaseName, "Book", NCS.ExportString(), NWS.ExportString(), strDoc, "system");

                try
                {
                    GRB_WebService.GRB_WebService GRB_ws = new GRB_WebService.GRB_WebService();
                    GRB_ws.DeleteDestroryByID(int.Parse(ID), "Expired");

                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("ERROR:" + ex.Message);
                 
                }
          
            }
        }
    }
}
