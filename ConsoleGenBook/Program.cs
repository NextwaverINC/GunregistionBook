using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConsoleGenBook
{
    class Program
    {

        static void Main(string[] args)
        {

            System.Collections.Generic.IEnumerable<String> lines = File.ReadLines("file.txt");

            foreach (var item in lines)
            {
                string Connection = "NextwaverDatabase";
                string OfficeSpaceId = "OF.0001";
                string DatabaseName = "GunBook";

                string RootPath = "//Document/Data/Section[@ID='1']/Items[@Name='GunBook']";

                string RootPathPage = "//Document/Data/Section[@ID='1']/Items[@Name='Page']";

                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;

                string bookno = getParam(item, "bookno");
                string bookyear = getParam(item, "bookyear");
                string gunregidstart = getParam(item, "gunregidstart");
                string gunregidend = getParam(item, "gunregidend");
                string pagetotal = getParam(item, "pagetotal");
                string gunregidprefix = getParam(item, "gunregidprefix");
                string dual = getParam(item, "dual");
                int n;
                if (bookno.Trim() != "" && pagetotal.Trim() != "" && int.TryParse(bookno, out n) && int.TryParse(bookyear, out n) && int.TryParse(pagetotal, out n))
                {
                    System.Console.WriteLine("Create Book " + bookno);
                    Boolean isSuccess = false;
                    for (int iindex = 0; iindex < 10; iindex++)
                    {
                        if (iindex > 1)
                            System.Console.WriteLine("Rerun Book :: " + bookno);
                        try
                        {
                            string NCS_Encrypt;
                            string NWS_Encrypt;
                            NextwaverDB.NColumns NCS_S;
                            DataTable dt;

                            WorkSpace.Service WS = new WorkSpace.Service();

                            NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
                            NWS.Add(new NextwaverDB.NWhere("BOOKNO", bookno));

                            NCS_S = new NextwaverDB.NColumns();
                            NCS_S.Add(new NextwaverDB.NColumn("BOOKNO"));

                            NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
                            NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);

                            dt = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "Book", NCS_Encrypt, NWS_Encrypt, "System");
                            if (dt.Rows.Count == 0)
                            {
                                XmlDocument xDoc = new XmlDocument();
                                xDoc.Load(@"tempdoc\Book.xml");

                                NextwaverDB.NColumns NCS = new NextwaverDB.NColumns();
                                NCS.Add(new NextwaverDB.NColumn("BOOKNO", bookno));
                                NCS.Add(new NextwaverDB.NColumn("BOOKYEAR", bookyear));
                                NCS.Add(new NextwaverDB.NColumn("GUNREGIDSTART", gunregidstart));
                                NCS.Add(new NextwaverDB.NColumn("GUNREGIDEND", gunregidend));
                                NCS.Add(new NextwaverDB.NColumn("PAGETOTAL", pagetotal));
                                NCS.Add(new NextwaverDB.NColumn("GUNREGIDPREFIX", gunregidprefix));
                                NCS.Add(new NextwaverDB.NColumn("BOOKSTATUS", "Create"));
                                NCS.Add(new NextwaverDB.NColumn("CREATEDATE", convertDatetime(DateTime.Now)));
                                NCS.Add(new NextwaverDB.NColumn("CREATEBY", "System"));
                                NCS.Add(new NextwaverDB.NColumn("UPDATEDATE", convertDatetime(DateTime.Now)));
                                NCS.Add(new NextwaverDB.NColumn("UPDATEBY", "System"));
                                
                                AddDataXmlNode(xDoc, RootPath + "/Item[@Name='BookNo']", bookno);
                                AddDataXmlNode(xDoc, RootPath + "/Item[@Name='BookYear']", bookyear);
                                AddDataXmlNode(xDoc, RootPath + "/Item[@Name='GunRegIDStart']", gunregidstart);
                                AddDataXmlNode(xDoc, RootPath + "/Item[@Name='GunRegIDEnd']", gunregidend);
                                AddDataXmlNode(xDoc, RootPath + "/Item[@Name='PageTotal']", pagetotal);
                                AddDataXmlNode(xDoc, RootPath + "/Item[@Name='GunRegIDPrefix']", gunregidprefix);
                                AddDataXmlNode(xDoc, RootPath + "/Item[@Name='BookStatus']", "Create");
                                AddDataXmlNode(xDoc, RootPath + "/Item[@Name='CreateDate']", convertDatetime(DateTime.Now));
                                AddDataXmlNode(xDoc, RootPath + "/Item[@Name='CreateBy']", "System");
                                AddDataXmlNode(xDoc, RootPath + "/Item[@Name='UpdateDate']", convertDatetime(DateTime.Now));
                                AddDataXmlNode(xDoc, RootPath + "/Item[@Name='UpdateBy']", "System");

                                string strDoc = xDoc.OuterXml;
                                string[] OP = WS.InsertData(Connection, OfficeSpaceId, DatabaseName, "Book", NCS.ExportString(), strDoc, "System");

                                if (OP[0].ToUpper() == "OK")
                                {

                                }
                                else
                                {
                                    File.AppendAllText("log.txt", Environment.NewLine + Environment.NewLine
                                        + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " :: " + OP[1]);
                                    System.Console.WriteLine(OP[1]);
                                    //isSuccess = false;
                                    break;
                                }
                            }

                            NWS = new NextwaverDB.NWheres();
                            NWS.Add(new NextwaverDB.NWhere("BOOKNO", bookno));
                            NWS.Add(new NextwaverDB.NWhere("PAGEVERSION", "1"));

                            NCS_S = new NextwaverDB.NColumns();
                            NCS_S.Add(new NextwaverDB.NColumn("BOOKNO"));
                            NCS_S.Add(new NextwaverDB.NColumn("PAGENO"));

                            NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
                            NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);



                            XmlDocument xDocPage = new XmlDocument();
                            xDocPage.Load(@"tempdoc\Page.xml");
                            AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='BookNo']", bookno);
                            AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='PageVersion']", "1");
                            AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='PageStatus']", "Create");
                            AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='CreateDate']", convertDatetime(DateTime.Now));
                            AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='CreateBy']", "System");
                            AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='UpdateDate']", convertDatetime(DateTime.Now));
                            AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='UpdateBy']", "System");

                            DataTable dtp;
                            dtp = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "Page", NCS_Encrypt, NWS_Encrypt, "System");
                            int MaxPageTotal = dtp.Rows.Count;

                            bool isuccess = true;
                            if (MaxPageTotal < int.Parse(pagetotal))
                            {
                                for (int i = (MaxPageTotal + 1); i <= int.Parse(pagetotal); i++)
                                {
                                    NextwaverDB.NColumns NCSPage = new NextwaverDB.NColumns();
                                    NCSPage.Add(new NextwaverDB.NColumn("BOOKNO", bookno));
                                    NCSPage.Add(new NextwaverDB.NColumn("PAGEVERSION", "1"));
                                    NCSPage.Add(new NextwaverDB.NColumn("PAGESTATUS", "Create"));
                                    NCSPage.Add(new NextwaverDB.NColumn("CREATEDATE", convertDatetime(DateTime.Now)));
                                    NCSPage.Add(new NextwaverDB.NColumn("CREATEBY", "System"));
                                    NCSPage.Add(new NextwaverDB.NColumn("UPDATEDATE", convertDatetime(DateTime.Now)));
                                    NCSPage.Add(new NextwaverDB.NColumn("UPDATEBY", "System"));
                                    NCSPage.Add(new NextwaverDB.NColumn("PAGENO", i.ToString()));

                                    AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='PageNo']", i.ToString());

                                    string strDocPage = xDocPage.OuterXml;
                                    string[] OPPage = WS.InsertData(Connection, OfficeSpaceId, DatabaseName, "Page", NCSPage.ExportString(), strDocPage, "System");

                                    if (OPPage[0].ToUpper() == "OK")
                                    {
                                        System.Console.WriteLine("Create Book " + bookno + " Page " + i.ToString());
                                    }
                                    else
                                    {
                                        File.AppendAllText("log.txt", Environment.NewLine + Environment.NewLine
                                            + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " :: " + OPPage[1]);

                                        System.Console.WriteLine(OPPage[1]);
                                        isuccess = false;
                                        break;
                                    }
                                }
                            }

                            System.Console.WriteLine("Create Book " + bookno + " Success");
                            isSuccess = isuccess;
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText("log.txt", Environment.NewLine + Environment.NewLine
                                           + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " :: Book:" + bookno + " --> " + ex.Message);
                            break;
                        }

                        if (isSuccess)
                            break;
                        else
                            System.Threading.Thread.Sleep(500);
                    }
                }
                //
            }
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
