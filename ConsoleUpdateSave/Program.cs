using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConsoleUpdateSave
{
    class Program
    {
        static void Main(string[] args)
        {
            string ResultTableName = "TableData";
            SqlConnection DBConnect = DBConnection.GetDBConnection();

            SqlCommand DBCommand = new SqlCommand();
            DBCommand.Connection = DBConnect;

            //DBCommand.CommandType = CommandType.Text;
            //DBCommand.CommandText = "select ID, BookNo, PageNo from [Page] P with(nolock)"
            //                        + " where not exists(select BookNo, PageNo from Record with(nolock)"
            //                        + " where BookNo = P.BookNo and PageNo = P.PageNo"
            //                        + " group by BookNo, PageNo)"
            //                        + " And BookNo in (select BookNo from Record with(nolock)"
            //                        + " group by BookNo)"
            //                        + " and BookNo>290"
            //                        //+ " and PageNo<152"
            //                        + " order by BookNo, PageNo";

            DBCommand.CommandType = CommandType.StoredProcedure;
            DBCommand.CommandText = "spGBGetChkStatus";

            DBCommand.Parameters.Add(new SqlParameter("@DateBegin", "0"));
            DBCommand.Parameters.Add(new SqlParameter("@DateEnd", "2560071415"));
            DBCommand.Parameters.Add(new SqlParameter("@Status", "Save"));

            DBCommand.CommandTimeout = 7200;

            SqlDataReader DBReader = default(SqlDataReader);
            DataTable TableResult = default(DataTable);
            try
            {
                DBConnect.Open();
                DBReader = DBCommand.ExecuteReader();

                if (DBReader.HasRows)
                {
                    TableResult = new DataTable();
                    TableResult.Load(DBReader);
                    TableResult.TableName = ResultTableName;

                }
                else
                {
                    TableResult = new DataTable();
                    TableResult.Load(DBReader);
                    TableResult.TableName = ResultTableName;
                }
                string Connection = "NextwaverDatabase";
                string OfficeSpaceId = "OF.0001";
                string DatabaseName = "GunBook";
                string RootPathPage = "//Document/Data/Section[@ID='1']/Items[@Name='Page']";
                XmlDocument xDoc;

                foreach (DataRow item in TableResult.Rows)
                {
                    string ID = item["ID"].ToString();
                    WorkSpace.Service WS = new WorkSpace.Service();
                    xDoc = new XmlDocument();

                    String strDoc = WS.SelectLastDocument(Connection, OfficeSpaceId, DatabaseName, "Page", int.Parse(ID), "System");
                    strDoc = strDoc.Replace("null", "");
                    xDoc.LoadXml(strDoc);

                    NextwaverDB.NColumns NCS = new NextwaverDB.NColumns();
                    NCS.Add(new NextwaverDB.NColumn("PAGESTATUS", "Submit"));

                    AddDataXmlNode(xDoc, RootPathPage + "/Item[@Name='PageStatus']", "Submit");

                    NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
                    NWS.Add(new NextwaverDB.NWhere("ID", ID));

                    strDoc = xDoc.OuterXml;

                    string[] OP = WS.UpdateData(Connection, OfficeSpaceId, DatabaseName, "Page", NCS.ExportString(), NWS.ExportString(), strDoc, "System");
                    if (OP[0].ToUpper() == "OK")
                    {
                        Console.WriteLine(ID + " ::Success");
                    }
                    else
                    {
                        Console.WriteLine(ID + " ::Fail");
                    }
                }

            }
            catch (Exception exDb)
            {

            }
            finally
            {
                DBConnect.Close();
            }

        }

        static void AddDataXmlNode(XmlDocument xmlDoc, string xPath, string sValue)
        {
            XmlNode nodeControl = xmlDoc.SelectSingleNode(xPath);
            nodeControl.Attributes["Value"].Value = sValue;
        }
    }
}
