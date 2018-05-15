using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace InsertIDGUN
{
    class Program
    {
        static void Main(string[] args)
        {
            string Connection = "NextwaverDatabase";
            string OfficeSpaceId = "OF.0001";
            string DatabaseName = "GunBook";
            string UserName = "System";

            string strHostName = System.Net.Dns.GetHostName();
            string pathImgGun = System.Configuration.ConfigurationSettings.AppSettings["pathImgGun"];
            bool isSuccess = false;

            String[] EditImageTemplate = System.IO.File.ReadAllLines("BookEdit.txt");
            
            //NextwaverDB.NWheres NWSTest = new NextwaverDB.NWheres();
            //NWSTest.Add(new NextwaverDB.NWhere("PAGESTATUS", "Create"));

            //DataTable dttest = WS.SelectAllColumnByWhere(Connection, OfficeSpaceId, DatabaseName, "Page", NWSTest.ExportString(), UserName);
            
            for (int index = 0; index < 20; index++)
            {
                try
                {
                    WorkSpace.Service WS = new WorkSpace.Service();
                    string RootPathPage = "//Document/Data/Section[@ID='1']/Items[@Name='Page']";

                    foreach (string item in EditImageTemplate)
                    {
                        string Bookno = getParam(item, "BOOKNO");
                        NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
                        NWS.Add(new NextwaverDB.NWhere("BOOKNO", Bookno));

                        NextwaverDB.NColumns NCS_S = new NextwaverDB.NColumns();
                        NCS_S.Add(new NextwaverDB.NColumn("ID"));
                        NCS_S.Add(new NextwaverDB.NColumn("BOOKNO"));
                        NCS_S.Add(new NextwaverDB.NColumn("PAGENO"));
                        NCS_S.Add(new NextwaverDB.NColumn("IMGURL"));

                        string NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
                        string NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);

                        DataTable dt = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "Page", NCS_Encrypt, NWS_Encrypt, UserName);

                        int intPageNo = 0;
                        int intRowInPage = 20;
                        int intBegin = 0;

                        int.TryParse(getParam(item, "BEGINID"), out intBegin);

                        int.TryParse(getParam(item, "ROWINPAGE"), out intRowInPage);

                        if (intRowInPage == 0)
                            intRowInPage = 20;

                        foreach (DataRow itemData in dt.Rows)
                        {
                            intPageNo = Convert.ToInt16(itemData["PAGENO"]);
                            if (itemData["IMGURL"].ToString().Trim() != "")
                            {
                                XmlDocument xDocPage = new XmlDocument();
                                String strDoc = WS.SelectLastDocument(Connection, OfficeSpaceId, DatabaseName, "Page", int.Parse(itemData["ID"].ToString()), UserName);
                                xDocPage.LoadXml(strDoc);

                                string XPathDataRecord = "//Document/Data/Section[@ID='2']/Items[@Name='RecordInfo']";
                                XmlNode nodeDataGrid = xDocPage.SelectSingleNode(XPathDataRecord);
                                XmlNodeList listItem = nodeDataGrid.SelectNodes("./Item");
                                if (listItem.Count == 0)
                                {
                                    NextwaverDB.NColumns NColumns = new NextwaverDB.NColumns();
                                    
                                    NColumns.Add(new NextwaverDB.NColumn("UPDATEDATE", convertDatetime(DateTime.Now)));
                                    NColumns.Add(new NextwaverDB.NColumn("UPDATEBY", UserName));
                                    
                                    AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='UpdateDate']", convertDatetime(DateTime.Now));
                                    AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='UpdateBy']", UserName);

                                    XmlAttribute att;
                                    XmlNode nodeItem;
                                    for (int i = intBegin + ((intPageNo - 1) * intRowInPage); i < (intPageNo * intRowInPage) + intBegin; i++)
                                    {
                                        nodeItem = xDocPage.CreateElement("Item");

                                        att = xDocPage.CreateAttribute("C00");
                                        att.Value = i.ToString();
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C01");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C02");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C03");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C04");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C05");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C06");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C07");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C08");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C09");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C10");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C11");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C12");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C13");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C14");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C15");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C16");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        att = xDocPage.CreateAttribute("C17");
                                        att.Value = "";
                                        nodeItem.Attributes.Append(att);

                                        nodeDataGrid.AppendChild(nodeItem);

                                    }

                                    string strDocPage = xDocPage.OuterXml;

                                    NextwaverDB.NWheres NWheres = new NextwaverDB.NWheres();
                                    NWheres.Add(new NextwaverDB.NWhere("ID", itemData["ID"].ToString()));

                                    string[] msgOutput = WS.UpdateData(Connection, OfficeSpaceId, DatabaseName, "Page", NColumns.ExportString(), NWheres.ExportString(), strDocPage, UserName);
                                    if (msgOutput[0].ToUpper() == "OK")
                                    {
                                        System.Console.WriteLine("Update ID -->> BookNo " + itemData["BOOKNO"] + " PageNo " + itemData["PAGENO"]);

                                    }
                                    else
                                    {
                                        File.AppendAllText("Log.txt", Environment.NewLine + Environment.NewLine
                                       + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " :: Book:" + itemData["BOOKNO"] + " :: Page:" + itemData["PAGENO"] + " --> " + msgOutput[1]);
                                    }
                                }
                                
                            }
                        }

                    }
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    File.AppendAllText("Log.txt", Environment.NewLine + Environment.NewLine
                                           + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " --> " + ex.Message);
                }
                if (isSuccess)
                    break;
                else
                    System.Threading.Thread.Sleep(500);
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
