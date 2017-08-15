using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;

namespace UpdateImg
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

            try
            {
                foreach (string dirPath in Directory.GetDirectories(@"Book\", "*", SearchOption.TopDirectoryOnly))
                {
                    Boolean isSuccess = false;
                    Exception err = new Exception();

                    int BookID = Convert.ToInt32(dirPath.Substring(dirPath.Length - 4));

                    for (int index = 0; index < 20; index++)
                    {
                        try
                        {
                            string Bookno = BookID.ToString();

                            WorkSpace.Service WS = new WorkSpace.Service();

                            string RootPathPage = "//Document/Data/Section[@ID='1']/Items[@Name='Page']";

                            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;

                            NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
                            NWS.Add(new NextwaverDB.NWhere("BOOKNO", Bookno));

                            NextwaverDB.NColumns NCS_S = new NextwaverDB.NColumns();
                            NCS_S.Add(new NextwaverDB.NColumn("ID"));
                            NCS_S.Add(new NextwaverDB.NColumn("BOOKNO"));
                            NCS_S.Add(new NextwaverDB.NColumn("PAGENO"));
                            NCS_S.Add(new NextwaverDB.NColumn("IMGURL"));

                            NextwaverDB.NColumns NCS_SY = new NextwaverDB.NColumns();
                            NCS_SY.Add(new NextwaverDB.NColumn("BOOKYEAR"));

                            string NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
                            string NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);

                            string NCSS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_SY.ExportString(), true);

                            DataTable dtBook = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "Book", NCSS_Encrypt, NWS_Encrypt, UserName);

                            if (dtBook.Rows.Count > 0)
                            {
                                string strBookno = "0000" + Bookno;

                                strBookno = strBookno.Substring(strBookno.Length - 4);

                                string dirPathimg = "\\\\" + strHostName + pathImgGun + "Active\\Y" + dtBook.Rows[0]["BOOKYEAR"].ToString() + "\\B" + strBookno;

                                if (!System.IO.Directory.Exists(dirPathimg))
                                    System.IO.Directory.CreateDirectory(dirPathimg);
                                
                                DataTable dt = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "Page", NCS_Encrypt, NWS_Encrypt, UserName);

                                string[] fileList = System.IO.Directory.GetFiles(dirPath, "*.jpg", SearchOption.TopDirectoryOnly);

                                string allNamefile = "";
                                foreach (string file in fileList)
                                {
                                    allNamefile += "," + Path.GetFileName(file);
                                };

                                foreach (DataRow item in dt.Rows)
                                {
                                    string pageNo = "0000" + item["PAGENO"].ToString();
                                    pageNo = pageNo.Substring(pageNo.Length - 4);

                                    string nameImg = "B" + strBookno + "_P" + pageNo + "_V01";

                                    bool chkName = allNamefile.Contains(nameImg);

                                    if (item["IMGURL"].ToString().Trim() == "" && chkName)
                                    {
                                        //Active/Y2519/B0122/B0122_P0001_V01.jpg
                                        if (System.IO.File.Exists(dirPathimg + "\\" + nameImg + ".jpg"))
                                            System.IO.File.Delete(dirPathimg + "\\" + nameImg + ".jpg");

                                        Image myImage = Image.FromFile(dirPath + "\\" + nameImg + ".jpg");
                                        myImage.Save(dirPathimg + "\\" + nameImg + ".jpg", ImageFormat.Jpeg);
                                        myImage.Dispose();
                                        System.Console.WriteLine("copy image -->> " + dirPathimg + "\\" + nameImg + ".jpg");

                                        string ImgURL = "Active/Y" + dtBook.Rows[0]["BOOKYEAR"].ToString() + "/B" + strBookno + "/" + nameImg + ".jpg";


                                        NextwaverDB.NColumns NColumns = new NextwaverDB.NColumns();
                                        NColumns.Add(new NextwaverDB.NColumn("IMGURL", ImgURL));
                                        NColumns.Add(new NextwaverDB.NColumn("PAGESTATUS", "Scan"));
                                        NColumns.Add(new NextwaverDB.NColumn("SCANDATE", convertDatetime(DateTime.Now)));
                                        NColumns.Add(new NextwaverDB.NColumn("SCANBY", UserName));
                                        NColumns.Add(new NextwaverDB.NColumn("UPDATEDATE", convertDatetime(DateTime.Now)));
                                        NColumns.Add(new NextwaverDB.NColumn("UPDATEBY", UserName));

                                        NextwaverDB.NWheres NWheres = new NextwaverDB.NWheres();
                                        NWheres.Add(new NextwaverDB.NWhere("ID", item["ID"].ToString()));

                                        XmlDocument xDocPage = new XmlDocument();
                                        String strDoc = WS.SelectLastDocument(Connection, OfficeSpaceId, DatabaseName, "Page", int.Parse(item["ID"].ToString()), UserName);
                                        xDocPage.LoadXml(strDoc);

                                        AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='ImgUrl']", ImgURL);
                                        AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='PageStatus']", "Scan");
                                        AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='ScanDate']", convertDatetime(DateTime.Now));
                                        AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='ScanBy']", UserName);
                                        AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='UpdateDate']", convertDatetime(DateTime.Now));
                                        AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='UpdateBy']", UserName);

                                        string strDocPage = xDocPage.OuterXml;

                                        string[] msgOutput = WS.UpdateData(Connection, OfficeSpaceId, DatabaseName, "Page", NColumns.ExportString(), NWheres.ExportString(), strDocPage, UserName);
                                        if (msgOutput[0].ToUpper() == "OK")
                                        {
                                            System.Console.WriteLine(item["ID"].ToString() + " Book:" + strBookno + " Page:" + pageNo + " Image:" + ImgURL);

                                        }
                                        else
                                        {
                                            File.AppendAllText("log.txt", Environment.NewLine + Environment.NewLine
                                           + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " :: Book:" + BookID.ToString() + " --> " + msgOutput[1]);
                                        }
                                    }
                                }

                                //DirectoryInfo dir = new DirectoryInfo(dirPath);
                                //FileInfo[] fis = dir.GetFiles("*", SearchOption.TopDirectoryOnly);

                                //foreach (FileInfo fi in fis)
                                //{
                                //    if (System.IO.File.Exists(dirPathimg + "\\" + fi.Name))
                                //        System.IO.File.Delete(dirPathimg + "\\" + fi.Name);

                                //    Image myImage = Image.FromFile(fi.FullName);

                                //    myImage.Save(dirPathimg + "\\" + fi.Name, ImageFormat.Jpeg);
                                //    //File.Copy(fi.FullName, dirPathimg + "\\" + fi.Name, true);
                                //    myImage.Dispose();
                                //    System.Console.WriteLine("copy::" + dirPathimg + "\\" + fi.Name);

                                //}

                                if (Directory.Exists(dirPath))
                                    Directory.Delete(dirPath, true);

                                System.Console.WriteLine("Success Book :" + Bookno);
                            }
                            isSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            isSuccess = false;
                            File.AppendAllText("log.txt", Environment.NewLine + Environment.NewLine
                                           + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " :: Book:" + BookID.ToString() + " --> " + ex.Message);
                        }

                        if (isSuccess)
                            break;
                        else
                            System.Threading.Thread.Sleep(500);
                    }

                }
                System.Console.WriteLine("Success");
                //System.Console.ReadKey();
            }
            catch (Exception ex)
            {
                File.AppendAllText("log.txt", Environment.NewLine + Environment.NewLine
                                       + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " --> " + ex.Message);
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
