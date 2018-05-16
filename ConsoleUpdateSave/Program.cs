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

            DBCommand.CommandType = CommandType.Text;
            DBCommand.CommandText = "SELECT BB.ID,BB.BookNo,BB.PageNo,AA.PageNo FROM" +
                                    " (SELECT [BookNo]" +
                                    " ,[PageNo]" +
                                    " FROM [GunRegBook].[dbo].[Record] With(nolock)" +
                                    " Group By [BookNo]" +
                                    " ,[PageNo]) AA" +
                                    " RIGHT JOIN" +
                                    " (SELECT [ID],[BookNo],[PageNo] FROM [dbo].[Page]" +
                                    " WHERE [BookNo] in" +
                                    " (Select distinct([BookNo])" +
                                    " FROM [GunRegBook].[dbo].[Record] With(nolock))) BB ON AA.BookNo = BB.BookNo and AA.PageNo = BB.PageNo" +
                                    " Where AA.PageNo is null" +
                                    " ORDER BY BB.BookNo,BB.PageNo";

            //DBCommand.CommandText = "SELECT *" +
            //                    " INTO #tmpData" +
            //                    " FROM (" +
            //                    " SELECT [BookNo],[GunRegID]" +
            //                    " , row_number() OVER(PARTITION BY [BookNo],[PageNo] ORDER BY [GunRegID]) AS [RowNo] FROM [GunRegBook].[dbo].[Record] With(nolock)" +
            //                    " WHERE [PageNo] = 1) C" +
            //                    " WHERE [RowNo] = 1" +
            //                    " SELECT BB.ID,BB.BookNo,BB.PageNo" +
            //                    " INTO #tmpData2" +
            //                    " FROM" +
            //                    " (SELECT [BookNo]" +
            //                    "      ,[PageNo]" +
            //                    "  FROM [GunRegBook].[dbo].[Record] With(nolock)" +
            //                    "  Group By[BookNo]" +
            //                    "      ,[PageNo]) AA" +
            //                    "      RIGHT JOIN" +
            //                    " (SELECT [ID],[BookNo],[PageNo] FROM [dbo].[Page]" +
            //                    " WHERE [BookNo] in" +
            //                    " (Select distinct([BookNo])" +
            //                    " FROM [GunRegBook].[dbo].[Record] With(nolock))) BB ON AA.BookNo = BB.BookNo and AA.PageNo = BB.PageNo" +
            //                    " Where AA.PageNo is null" +
            //                    " ORDER BY BB.BookNo,BB.PageNo" +
            //                    " SELECT BB.*,AA.GunRegID FROM #tmpData AA" +
            //                    " right join #tmpData2 BB ON AA.BookNo=BB.BookNo" +
            //                    " Order By BB.BookNo,BB.PageNo" +
            //                    " DROP TABLE #tmpData" +
            //                    " DROP TABLE #tmpData2";

            //DBCommand.Parameters.Add(new SqlParameter("@DateBegin", "0"));
            //DBCommand.Parameters.Add(new SqlParameter("@DateEnd", "2560071415"));
            //DBCommand.Parameters.Add(new SqlParameter("@Status", "Save"));

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

                    string BOOKNO = "" + GetDataXmlNode(xDoc, RootPathPage + "/Item[@Name='BookNo']");
                    string PAGENO = "" + GetDataXmlNode(xDoc, RootPathPage + "/Item[@Name='PageNo']");
                    string STATUS = "" + GetDataXmlNode(xDoc, RootPathPage + "/Item[@Name='PageStatus']");
                    string IMGURL = "" + GetDataXmlNode(xDoc, RootPathPage + "/Item[@Name='ImgUrl']");

                    if (BOOKNO == item["BookNo"].ToString() && PAGENO == item["PageNo"].ToString())
                    {
                        string XPathDataRecord = "//Document/Data/Section[@ID='2']/Items[@Name='RecordInfo']";
                        XmlNode nodeDataGrid = xDoc.SelectSingleNode(XPathDataRecord);
                        XmlNodeList listItem = nodeDataGrid.SelectNodes("./Item");
                        if (listItem.Count > 0 && listItem.Count <= 20)
                        {
                            NextwaverDB.NColumns NCS = new NextwaverDB.NColumns();
                            NCS.Add(new NextwaverDB.NColumn("PAGESTATUS", STATUS));

                            NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
                            NWS.Add(new NextwaverDB.NWhere("ID", ID));

                            strDoc = xDoc.OuterXml;

                            string[] OP = WS.UpdateData(Connection, OfficeSpaceId, DatabaseName, "Page", NCS.ExportString(), NWS.ExportString(), strDoc, "System");
                            if (OP[0].ToUpper() == "OK")
                            {
                                Console.WriteLine(ID + "Book " + BOOKNO + " Page " + PAGENO + " ::Success");
                            }
                            else
                            {
                                Console.WriteLine(ID + "Book " + BOOKNO + " Page " + PAGENO + " ::Fail");
                            }
                        }
                        
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
        static string GetDataXmlNode(XmlDocument xmlDoc, string xPath)
        {
            XmlNode nodeControl = xmlDoc.SelectSingleNode(xPath);
            return "" + nodeControl.Attributes["Value"].Value;
        }
    }
}
