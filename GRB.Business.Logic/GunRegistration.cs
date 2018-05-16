using System;
using System.Data;
using GRB_Dat = GRB.DataAccess;
using System.Xml;
using System.Configuration;
using System.IO;
using System.Drawing;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Web;

namespace GRB.Business.Logic
{
    public class GunRegistration
    {
        static string Connection = "NextwaverDatabase";
        static string OfficeSpaceId = "OF.0001";
        static string DatabaseName = "GunBook";
        static string pathImgGun = ConfigurationManager.AppSettings["pathImgGun"];
        static string pathImgGunReplace = ConfigurationManager.AppSettings["pathImgGunReplace"];
        static string pathMaskLine = ConfigurationManager.AppSettings["pathMaskLine"];
        static string pathWaterMask = ConfigurationManager.AppSettings["pathWaterMask"];
        
        static string strHostName = System.Net.Dns.GetHostName();
        public static DataTable UserAuth(string UserName, string Password)
        {
            try
            {
                NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
                NWS.Add(new NextwaverDB.NWhere("USERNAME", UserName));
                NWS.Add(new NextwaverDB.NWhere("PASSWORD", Password));

                NextwaverDB.NColumns NCS_S = new NextwaverDB.NColumns();
                NCS_S.Add(new NextwaverDB.NColumn("USERNAME"));
                NCS_S.Add(new NextwaverDB.NColumn("POSITION_CODE"));
                NCS_S.Add(new NextwaverDB.NColumn("ID"));

                WorkSpace.Service WS = new WorkSpace.Service();

                string NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
                string NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);

                DataTable dt = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, "desktop", "users", NCS_Encrypt, NWS_Encrypt, "system");
                if (dt.Rows.Count == 0)
                {
                    throw new Exception("UserName หรือ Password ไม่ถูกต้อง");
                }
                else
                    return dt;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void AddDataXmlNode(XmlDocument xmlDoc, string xPath, string sValue)
        {
            XmlNode nodeControl = xmlDoc.SelectSingleNode(xPath);
            nodeControl.Attributes["Value"].Value = sValue;
        }

        public static DataTable GetBook(Int32 BookNo)
        {
            try
            {
                return GRB_Dat.spGRBGetBook.ExecuteReader(BookNo);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable GetBookByCondition(Int32 BookNo, Int32 BookYear)
        {
            try
            {
                return GRB_Dat.spGRBGetBookByCondition.ExecuteReader(BookNo, BookYear);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable GetPage(Int32 BookNo, Int32 PageNo)
        {
            try
            {
                return GRB_Dat.spGRBGetPage.ExecuteReader(BookNo, PageNo);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable GetPageVer(Int32 BookNo, Int32 PageNo, Int32 Version)
        {
            try
            {
                return GRB_Dat.spGRBGetPageVer.ExecuteReader(BookNo, PageNo, Version);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool SaveImagePage(Int32 BookNo, Int32 PageNo, Int32 Version, string ImgURL, string PathXml, string UserName, byte[] imageByte)
        {
            WorkSpace.Service WS = new WorkSpace.Service();
            XmlDocument xDocPage;
            string RootPathPage = "//Document/Data/Section[@ID='1']/Items[@Name='Page']";

            NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
            NWS.Add(new NextwaverDB.NWhere("BOOKNO", BookNo.ToString()));
            NWS.Add(new NextwaverDB.NWhere("PAGENO", PageNo.ToString()));
            NWS.Add(new NextwaverDB.NWhere("PAGEVERSION", Version.ToString()));

            NextwaverDB.NColumns NCS_S = new NextwaverDB.NColumns();
            NCS_S.Add(new NextwaverDB.NColumn("ID"));
            NCS_S.Add(new NextwaverDB.NColumn("BOOKNO"));
            NCS_S.Add(new NextwaverDB.NColumn("PAGENO"));
            NCS_S.Add(new NextwaverDB.NColumn("PAGEVERSION"));

            string NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
            string NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);

            DataTable dt = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "Page", NCS_Encrypt, NWS_Encrypt, UserName);
            if (dt.Rows.Count > 0)
            {
                NextwaverDB.NColumns NColumns = new NextwaverDB.NColumns();
                NColumns.Add(new NextwaverDB.NColumn("IMGURL", ImgURL));
                NColumns.Add(new NextwaverDB.NColumn("PAGESTATUS", "Scan"));
                NColumns.Add(new NextwaverDB.NColumn("SCANDATE", convertDatetime(DateTime.Now)));
                NColumns.Add(new NextwaverDB.NColumn("SCANBY", UserName));
                NColumns.Add(new NextwaverDB.NColumn("UPDATEDATE", convertDatetime(DateTime.Now)));
                NColumns.Add(new NextwaverDB.NColumn("UPDATEBY", UserName));

                NextwaverDB.NWheres NWheres = new NextwaverDB.NWheres();
                NWheres.Add(new NextwaverDB.NWhere("ID", dt.Rows[0]["ID"].ToString()));

                xDocPage = new XmlDocument();
                String strDoc = WS.SelectLastDocument(Connection, OfficeSpaceId, DatabaseName, "Page", int.Parse(dt.Rows[0]["ID"].ToString()), UserName);
                xDocPage.LoadXml(strDoc);

                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='ImgUrl']", ImgURL);
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='PageStatus']", "Scan");
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='ScanDate']", convertDatetime(DateTime.Now));
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='ScanBy']", UserName);
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='UpdateDate']", convertDatetime(DateTime.Now));
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='UpdateBy']", UserName);

                MemoryStream stream = new MemoryStream(imageByte);

                Image myImage = Image.FromStream(stream);

                Boolean saveImgResult = ImageUtil.SaveImage("//" + strHostName + "/" + pathImgGun + ImgURL, myImage);

                string strDocPage = xDocPage.OuterXml;

                string[] msgOutput = WS.UpdateData(Connection, OfficeSpaceId, DatabaseName, "Page", NColumns.ExportString(), NWheres.ExportString(), strDocPage, UserName);
                if (msgOutput[0].ToUpper() == "OK") return true;
                else
                {
                    throw new Exception(msgOutput[1]);
                }
            }
            else
            {
                NextwaverDB.NColumns NCSPage = new NextwaverDB.NColumns();
                NCSPage.Add(new NextwaverDB.NColumn("BOOKNO", BookNo.ToString()));
                NCSPage.Add(new NextwaverDB.NColumn("PAGENO", PageNo.ToString()));
                NCSPage.Add(new NextwaverDB.NColumn("PAGEVERSION", Version.ToString()));
                NCSPage.Add(new NextwaverDB.NColumn("IMGURL", ImgURL));
                NCSPage.Add(new NextwaverDB.NColumn("PAGESTATUS", "Scan"));
                NCSPage.Add(new NextwaverDB.NColumn("SCANDATE", convertDatetime(DateTime.Now)));
                NCSPage.Add(new NextwaverDB.NColumn("SCANBY", UserName));
                NCSPage.Add(new NextwaverDB.NColumn("CREATEDATE", convertDatetime(DateTime.Now)));
                NCSPage.Add(new NextwaverDB.NColumn("CREATEBY", UserName));
                NCSPage.Add(new NextwaverDB.NColumn("UPDATEDATE", convertDatetime(DateTime.Now)));
                NCSPage.Add(new NextwaverDB.NColumn("UPDATEBY", UserName));

                xDocPage = new XmlDocument();

                if (Version == 1)
                {
                    xDocPage.Load(PathXml);
                }
                else
                {
                    string Sql = @"SELECT [ID]
                                      FROM [GunRegBook].[dbo].[Page]
                                      WHERE BookNo = @BookNo 
                                      AND PageNo = @PageNo
                                      AND PageVersion = @Version";
                    Sql = Sql.Replace("@BookNo", "" + BookNo);
                    Sql = Sql.Replace("@PageNo", "" + PageNo);
                    Sql = Sql.Replace("@Version", "" + (Version - 1));
                    DataTable dt_old = GRB_Dat.DBExecute.ExecuteTextQuery(Sql);
                    String strDoc = WS.SelectLastDocument(Connection, OfficeSpaceId, DatabaseName, "Page", int.Parse(dt_old.Rows[0]["ID"].ToString()), UserName);
                    xDocPage.LoadXml(strDoc);
                }
                

                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='BookNo']", BookNo.ToString());
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='PageNo']", PageNo.ToString());
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='PageVersion']", Version.ToString());
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='ImgUrl']", ImgURL);
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='PageStatus']", "Scan");
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='ScanDate']", convertDatetime(DateTime.Now));
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='ScanBy']", UserName);
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='CreateDate']", convertDatetime(DateTime.Now));
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='CreateBy']", UserName);
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='UpdateDate']", convertDatetime(DateTime.Now));
                AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='UpdateBy']", UserName);

                MemoryStream stream = new MemoryStream(imageByte);

                Image myImage = Image.FromStream(stream);

                Boolean saveImgResult = ImageUtil.SaveImage("//" + strHostName + "/" + pathImgGun + ImgURL, myImage);

                string strDocPage = xDocPage.OuterXml;
                string[] OPPage = WS.InsertData(Connection, OfficeSpaceId, DatabaseName, "Page", NCSPage.ExportString(), strDocPage, UserName);
                if (OPPage[0].ToUpper() == "OK") return true;
                else
                {
                    throw new Exception(OPPage[1]);
                }
            }
        }

        public static byte[] getimg(string strpath)
        {
            MemoryStream ms = new MemoryStream();
            Image img = Image.FromFile("//" + strHostName + "/" + pathImgGun + strpath);
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            return ms.ToArray();
        }

        public static byte[] getimg(Int32 BookNo, Int32 PageNo)
        {
            WorkSpace.Service WS = new WorkSpace.Service();

            NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();

            NWS.Add(new NextwaverDB.NWhere("BOOKNO", BookNo.ToString()));
            NWS.Add(new NextwaverDB.NWhere("PAGENO", PageNo.ToString()));

            NextwaverDB.NColumns NCS_S = new NextwaverDB.NColumns();
            NCS_S.Add(new NextwaverDB.NColumn("PAGEVERSION"));
            NCS_S.Add(new NextwaverDB.NColumn("IMGURL"));

            string NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
            string NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);

            DataTable dt = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "Page", NCS_Encrypt, NWS_Encrypt, "System");

            string strpath = "";
            int mostVer = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i == 0)
                {
                    mostVer = Convert.ToInt16(dt.Rows[i]["PAGEVERSION"].ToString());
                    strpath = dt.Rows[i]["IMGURL"].ToString();
                }
                else
                {
                    if (mostVer < Convert.ToInt16(dt.Rows[i]["PAGEVERSION"].ToString()))
                    {
                        mostVer = Convert.ToInt16(dt.Rows[i]["PAGEVERSION"].ToString());
                        strpath = dt.Rows[i]["IMGURL"].ToString();
                    }
                }
            }
            if (strpath == "")
            {
                throw new Exception("Not found page image data.");
            }
            else
            {
                try
                {
                    MemoryStream ms = new MemoryStream();
                    Image img = Image.FromFile("//" + strHostName + "/" + pathImgGun + strpath);
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                    return ms.ToArray();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }

        }

        public static string getimgURL(Int32 BookNo, Int32 PageNo, Int32 Ver)
        {
            WorkSpace.Service WS = new WorkSpace.Service();

            NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
            NWS.Add(new NextwaverDB.NWhere("BOOKNO", BookNo.ToString()));
            NWS.Add(new NextwaverDB.NWhere("PAGENO", PageNo.ToString()));
            NWS.Add(new NextwaverDB.NWhere("PAGEVERSION", Ver.ToString()));

            NextwaverDB.NColumns NCS_S = new NextwaverDB.NColumns();
            NCS_S.Add(new NextwaverDB.NColumn("IMGURL"));

            string NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
            string NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);

            DataTable dt = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "Page", NCS_Encrypt, NWS_Encrypt, "System");

            string strpath = dt.Rows[0]["IMGURL"].ToString();

            return strpath;

        }

        public static byte[] getimg(Int32 BookNo, Int32 PageNo, Int32 Ver)
        {
            WorkSpace.Service WS = new WorkSpace.Service();

            NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
            NWS.Add(new NextwaverDB.NWhere("BOOKNO", BookNo.ToString()));
            NWS.Add(new NextwaverDB.NWhere("PAGENO", PageNo.ToString()));
            NWS.Add(new NextwaverDB.NWhere("PAGEVERSION", Ver.ToString()));

            NextwaverDB.NColumns NCS_S = new NextwaverDB.NColumns();
            NCS_S.Add(new NextwaverDB.NColumn("IMGURL"));

            string NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
            string NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);

            DataTable dt = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "Page", NCS_Encrypt, NWS_Encrypt, "System");

            string strpath = dt.Rows[0]["IMGURL"].ToString();

            try
            {
                //MemoryStream ms = new MemoryStream();
                //Image img = Image.FromFile("//" + strHostName + "/" + pathImgGun + strpath);
                //img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                //----------------------------RESIZE----------------------
                MemoryStream ms = new MemoryStream();

                //    Image img = Image.FromFile("//" + strHostName + "/" + pathImgGun + strpath);
 
                Image myImage = Image.FromFile("//" + strHostName + "/" + pathImgGun + strpath);





                ResizeBilinear ResizeImage = new ResizeBilinear(3000, 4200);

                myImage = ResizeImage.Apply((Bitmap)myImage);



                //---------Convert GrayScale Mode----
                // Turn image into gray scale image
                Image grayScaled = (Image)MakeGrayscale(new Bitmap(myImage));

                using (Bitmap newBitmap = new Bitmap(grayScaled))
                {
                    newBitmap.SetResolution(300, 300);
                    //      newBitmap.Save(result + "\\" + fileName, ImageFormat.Jpeg);

                    newBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                }
                //-------------------------------


              //  myImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                //-----------------------------

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static string convertDatetime(DateTime date)
        {
            int yearTh = date.Year;
            if (yearTh < 2400)
                yearTh = yearTh + 543;

            return date.Day.ToString() + "/" + (date.Month + 1).ToString() + "/" + yearTh.ToString() + " " + date.Hour.ToString() + ":" + date.Minute.ToString() + ":" + date.Second.ToString();
        }

        public static bool EditPageImg(Int32 BookNo)
        {
            WorkSpace.Service WS = new WorkSpace.Service();
            XmlDocument xDocPage;
            string RootPathPage = "//Document/Data/Section[@ID='1']/Items[@Name='Page']";

            NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
            NWS.Add(new NextwaverDB.NWhere("BOOKNO", BookNo.ToString()));

            NextwaverDB.NColumns NCS_S = new NextwaverDB.NColumns();
            NCS_S.Add(new NextwaverDB.NColumn("ID"));
            NCS_S.Add(new NextwaverDB.NColumn("BOOKNO"));
            NCS_S.Add(new NextwaverDB.NColumn("IMGURL"));

            string NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
            string NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);

            DataTable dt = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "Page", NCS_Encrypt, NWS_Encrypt, "system");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string imgNewPath = dt.Rows[i]["IMGURL"].ToString();
                    imgNewPath = imgNewPath.Replace(pathImgGunReplace, "");

                    NextwaverDB.NColumns NColumns = new NextwaverDB.NColumns();
                    NColumns.Add(new NextwaverDB.NColumn("IMGURL", imgNewPath));

                    NextwaverDB.NWheres NWheres = new NextwaverDB.NWheres();
                    NWheres.Add(new NextwaverDB.NWhere("ID", dt.Rows[i]["ID"].ToString()));

                    xDocPage = new XmlDocument();
                    string strDoc = WS.SelectLastDocument(Connection, OfficeSpaceId, DatabaseName, "Page", int.Parse(dt.Rows[i]["ID"].ToString()), "system");
                    string strDocReplaceNull = strDoc.Replace("null", "");
                    xDocPage.LoadXml(strDocReplaceNull);

                    AddDataXmlNode(xDocPage, RootPathPage + "/Item[@Name='ImgUrl']", imgNewPath);

                    string strDocPage = xDocPage.OuterXml;

                    string[] msgOutput = WS.UpdateData(Connection, OfficeSpaceId, DatabaseName, "Page", NColumns.ExportString(), NWheres.ExportString(), strDocPage, "system");
                    if (msgOutput[0].ToUpper() == "OK")
                    {
                    }
                    else
                    {
                        throw new Exception(msgOutput[1]);
                    }
                }
                return true;
            }
            else
            {
                throw new Exception("not record");
            }
        }

        public static DataTable GetTblGunReg(Int32 BookNo, Int32 PageNo, Int32 PageVersion, string GunRegID, string GunNo
    , string GunGroup, string GunType, string GunSize, string GunBrand, Int32 GunMaxShot, string GunBarrel, string GunColor
    , string GunOwner, string GunRegDate, string GunRemark, string GunCountry,int GunIdOnly)
        {
            try
            {
                return GRB_Dat.spGBGetTblGunRegistration.ExecuteReader(BookNo, PageNo, PageVersion, GunRegID, GunNo
    , GunGroup, GunType, GunSize, GunBrand, GunMaxShot, GunBarrel, GunColor
    , GunOwner, GunRegDate, GunRemark, GunCountry, GunIdOnly);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable GetDataGun(string DateBegin, string DateEnd, string pStatus)
        {
            try
            {
                return GRB_Dat.spGBGetTblData.ExecuteReader(DateBegin, DateEnd, pStatus);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable GetDataGunSum(string DateBegin, string DateEnd, string pStatus)
        {
            try
            {
                return GRB_Dat.spGBGetTblDataSum.ExecuteReader(DateBegin, DateEnd, pStatus);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void logUserGun(string UserName, string MenuUse, string UserProcess, bool ProcessResult, string paramsList)
        {
            try
            {
                string strResult = "Unsuccess";
                if (ProcessResult)
                    strResult = "success";

                GRB_Dat.spGRBCreateLog.ExecuteReader(UserName, MenuUse, UserProcess, strResult, paramsList);
            }
            catch (Exception ex)
            {
                throw ex;
            }


            //WorkSpace.Service WS = new WorkSpace.Service();

            //NextwaverDB.NColumns NCS = new NextwaverDB.NColumns();

            //string strResult = "Unsuccess";
            //if (ProcessResult)
            //    strResult = "success";

            ////NCS.Add(new NextwaverDB.NColumn("USERNAME", UserName));
            ////NCS.Add(new NextwaverDB.NColumn("MENU", MenuUse));
            ////NCS.Add(new NextwaverDB.NColumn("PROCESS", UserProcess));
            ////NCS.Add(new NextwaverDB.NColumn("RESULT", strResult));
            ////NCS.Add(new NextwaverDB.NColumn("PARAM", paramsList));
            ////NCS.Add(new NextwaverDB.NColumn("DATETIME", DateTime.Now.ToString("F")));

            //string[] OP = WS.InsertData(Connection, OfficeSpaceId, DatabaseName, "LogUser", NCS.ExportString(), "", "System");
        }

        public static byte[] getPdf(string strpath)
        {
            byte[] bytes = System.IO.File.ReadAllBytes("//" + strHostName + "/" + strpath);

            return bytes;
        }

        public static bool updatePercentErr(Int32 BookNo, Int32 PageNo, decimal PercentErr)
        {
            string strUpdatePercent = "UPDATE Page SET PercentErr = " + PercentErr.ToString("#.##") + " WHERE BookNo = " + BookNo.ToString() + " And PageNo = " + PageNo.ToString();
            try
            {
                return GRB_Dat.DBExecute.ExecuteTextNonQuery(strUpdatePercent);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool insertLog(string UserName, string Pocess, string Detail, string DateUpdate)
        {
            string strInsert = "INSERT INTO[dbo].[Logs] ([UserName],[Pocess],[Detail],[DateUpdate]) VALUES('" + UserName + "' ,'" + Pocess + "' ,'" + Detail + "','" + DateUpdate + "')";
            try
            {
                return GRB_Dat.DBExecute.ExecuteTextNonQuery(strInsert);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool insertImg(Int32 BookNo, Int32 PageNo, string Detail)
        {
            string strInsert = "INSERT INTO [dbo].[img_report] ([BookNo] ,[Page] ,[Detail]) VALUES ( "+ BookNo.ToString() + ", "+ PageNo.ToString() + ",'"+ Detail + "')";
            try
            {
                return GRB_Dat.DBExecute.ExecuteTextNonQuery(strInsert);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static DataTable reportImgErr()
        {
            string strSelectData = "SELECT [BookNo] ,[Page] ,[Detail] FROM [GunRegBook].[dbo].[img_report]";
            try
            {
                return GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);
            }
            catch (Exception) { 
           
                throw;
            }
        }

        public static DataTable reportLog()
        {
            string strSelectData = "SELECT [UserName] ,[Menu] ,[Process] ,[Result],[Param],[CreateDate] FROM [GunRegBook].[dbo].[Logs] ORDER BY CreateDate DESC ";
            try
            {
                return GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static DataTable countAllData()
        {
            string strSelectData = "SELECT [BookAll], [PageAll], [RecodeAll] FROM [GunRegBook].[dbo].[ViewTotal]";
            try
            {
                return GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static DataTable countAllDataByBook()
        {
            string strSelectData = "SELECT [BookNo] ,[BookYear] ,[PageTotal] ,[TotalPage] ,[ImgTotal] ,[CountPage] ,[SumRecord]"
                                    + " FROM [GunRegBook].[dbo].[ViewTotalOfBook] WHERE[SumRecord] IS NOT NULL";
            try
            {
                return GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static DataTable countAllDataByYear()
        {
            string strSelectData = "SELECT [BookYear], COUNT([BookNo]) CountBook, SUM([ImgTotal]) SumPage, SUM([SumRecord]) SumRecode"
                                    + " FROM [GunRegBook].[dbo].[ViewTotalOfBook] WHERE[SumRecord] IS NOT NULL GROUP BY[BookYear] ORDER BY[BookYear]";
            try
            {
                return GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static DataTable API_getRecord(string GunRegID)
        {
            string strSelectData = @"SELECT top 1 r.BookNo,r.PageNo,r.PageVersion
                                        FROM Book b join record r on b.BookNo = r.BookNo 
                                        WHERE b.GunRegIDPrefix+r.GunRegID = '@GunRegID'
                                        ORDER BY r.PageVersion desc";


            strSelectData = strSelectData.Replace("@GunRegID", GunRegID);

            try
            {
                return GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);
            }
            catch (Exception)
            {
                throw;
            }
        }


        #region GetImageHide Old

       public static DataTable getDetailPage2(string bookno, string pageno, string pagever)
        {
            string strSelectData = @"SELECT *
                                    FROM Record
                                    WHERE BookNo = '@bookno' AND PageNo='@pageno' AND PageVersion = '@pagever'";


            strSelectData = strSelectData.Replace("@bookno", bookno).Replace("@pageno", pageno).Replace("@pagever", pagever);

            try
            {
                return GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static byte[] getImgHide2(Int32 BookNo, Int32 PageNo, Int32 Ver, string LineNo)
        {


            WorkSpace.Service WS = new WorkSpace.Service();
            NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
            NWS.Add(new NextwaverDB.NWhere("BOOKNO", BookNo.ToString()));
            NWS.Add(new NextwaverDB.NWhere("PAGENO", PageNo.ToString()));
            NWS.Add(new NextwaverDB.NWhere("PAGEVERSION", Ver.ToString()));

            NextwaverDB.NColumns NCS_S = new NextwaverDB.NColumns();
            NCS_S.Add(new NextwaverDB.NColumn("IMGURL"));

            string NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
            string NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);

            DataTable dt = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "Page", NCS_Encrypt, NWS_Encrypt, "System");

            string strpath = dt.Rows[0]["IMGURL"].ToString();

            try
            {
                MemoryStream ms = new MemoryStream();

                string ImageBack = "//" + strHostName + "/" + pathImgGun + strpath;
                //  string ImageFore = "//DESKTOP-O3I9FMD/Mask_Line/Line_1.png";
                // string ImageFore = "D:\\Mask_Line\\Line_" + LineNo + ".png"; 
                string ImageFore = "//" + strHostName + "/" + pathMaskLine + "Line_" + LineNo + ".png";
                //-------------Resize Image---------------------------------
                MemoryStream ms02 = new MemoryStream();

                Image m2 = Image.FromFile(ImageBack);

                Image myImage = Image.FromFile(ImageBack);
                ResizeBilinear ResizeImage = new ResizeBilinear(3000, 4200);

                myImage = ResizeImage.Apply((Bitmap)myImage);
                m2 = ResizeImage.Apply((Bitmap)m2);

                Image imgF = Image.FromFile(ImageFore);
                imgF = ResizeImage.Apply((Bitmap)imgF);

                //----------------Protect Image---------------------------------

                System.Drawing.Graphics myGraphic = null;

                Image imgB;// =new Image.FromFile(ImageBack);
                imgB = Image.FromFile(ImageBack);

                //Image imgF;// =new Image.FromFile(ImageBack);
                //imgF = Image.FromFile(ImageFore);

                Image m;
                m = Image.FromFile(ImageBack);
                // 

                myGraphic = System.Drawing.Graphics.FromImage(myImage);
                myGraphic.DrawImageUnscaled(myImage, 0, 0);
                myGraphic.DrawImageUnscaled(imgF, 0, 0);
                myGraphic.Save();

                //   m.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);


                //---------Convert GrayScale Mode----
                // Turn image into gray scale image
                Image grayScaled = (Image)MakeGrayscale(new Bitmap(myImage));

                using (Bitmap newBitmap = new Bitmap(grayScaled))
                {
                    newBitmap.SetResolution(300, 300);
                    //      newBitmap.Save(result + "\\" + fileName, ImageFormat.Jpeg);

                    newBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                }
                //-------------------------------

             //   myImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                myImage.Dispose();





                return ms.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion


        #region GetImageHide New

        public static DataTable getDetailPage(string bookno, string pageno, string pagever)
        {
                string strSelectData = @"SELECT *
                                    FROM [dbo].[IndexGunRegIDPrefix]
                                    WHERE BookNo = '@bookno' AND PageNo='@pageno' AND PageVersion = '@pagever'
                                    ORDER BY  PageNo,GunRegID";
                        


            strSelectData = strSelectData.Replace("@bookno", bookno).Replace("@pageno", pageno).Replace("@pagever", pagever);

            try
            {
                return GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static string ConfigMaskTarget(string BookNo)
        {
            string maskType = "";
            /*
            Find Mask target Config Book&Page
            */
            string[] mask01 = { "0001", "0016", "0018", "0021", "0022", "0024", "0025", "0026", "0028", "0029", "0030", "0035", "0036", "0038", "0039", "0040", "0041", "0042", "0043", "0044", "0046", "0049", "0052", "0053", "0056", "0057", "0059", "0060", "0061", "0062", "0063", "0064", "0067", "0068", "0070", "0071", "0072", "0073", "0074", "0075", "0078", "0081", "0082", "0083", "0084", "0085", "0092", "0143", "0144", "0145", "0146", "0147", "0148", "0149", "0151", "0152", "0153", "0154", "0155", "0156", "0157", "0159", "0160", "0161", "0162", "0164", "0166", "0167", "0168", "0169", "0170", "0171", "0222", "0223", "0224", "0226", "0227", "0228", "0229", "0230", "0231", "0232", "0233", "0234", "0235", "0236", "0238", "0239", "0240", "0241", "0242", "0243", "0244", "0287", "0292", "0294", "0295", "0296", "0299", "0300", "0301", "0302", "0303", "0304", "0305", "0306", "0307", "0308", "0309", "0310", "0314", "0318", "0319", "0320", "0321", "0322", "0324", "0327", "0331", "0332", "0335", "0336", "0338", "0339", "0340", "0341", "0342", "0343", "0344", "0345", "0346", "0347", "0348", "0349", "1501", "1502", "1901", "3701", "3702" };
            string[] mask02 = { "0058", "0065", "0066", "0069", "0076", "0079", "0086", "0086", "0087", "0088", "0088", "0089", "0090", "0091", "0093", "0094", "0095", "0096", "0097", "0098", "0099", "0100", "0101", "0102", "0103", "0104", "0105", "0106", "0107", "0109", "0110", "0111", "0112", "0113", "0115", "0116", "0117", "0118", "0119", "0120", "0121", "0122", "0123", "0124", "0125", "0126", "0127", "0128", "0129", "0130", "0131", "0132", "0133", "0134", "0135", "0136", "0137", "0138", "0139", "0140", "0141", "0142", "0150", "0165", "0172", "0173", "0174", "0175", "0176", "0177", "0178", "0179", "0180", "0181", "0182", "0183", "0184", "0185", "0186", "0187", "0188", "0189", "0190", "0191", "0192", "0193", "0194", "0195", "0196", "0197", "0198", "0199", "0200", "0201", "0202", "0203", "0204", "0205", "0206", "0207", "0208", "0209", "0210", "0212", "0213", "0215", "0216", "0217", "0221", "0237", "0245", "0246", "0247", "0248", "0249", "0250", "0251", "0252", "0253", "0254", "0255", "0256", "0257", "0258", "0259", "0260", "0261", "0262", "0263", "0264", "0265", "0266", "0267", "0268", "0269", "0270", "0271", "0272", "0273", "0274", "0275", "0276", "0277", "0278", "0279", "0280", "0281", "0282", "0283", "0284", "0285", "0286", "0288", "0289", "0290", "0291", "0293", "0297", "0311", "0312", "0313", "0315", "0316", "0323", "0325", "0326", "0328", "0329", "0330", "0337", "0350", "0352", "0353", "0354", "0355", "0356", "0357", "0358", "0359", "0360", "0361", "0362", "0363", "0365", "0367", "0368", "0371", "0373", "0374", "0375", "0376", "0377", "0378", "0379", "0380", "0381", "0382", "0383", "0384", "0385", "0386", "0387", "0388", "0389", "0390", "0391", "0392", "0393", "0394", "0395", "0396", "0397", "0398", "0399", "0400", "0401", "0402", "0403", "0404", "0405", "0406", "0407", "0408", "0409", "0410", "0411", "0412", "0413", "0414", "0415", "0416", "0417", "0418", "0419", "0420", "0421", "0422", "0423", "0424", "0425", "0426", "1701", "1702", "2910", "3102", "4470" };
            string[] mask03 = { "0027", "0033", "0034", "0037", "0045", "0047", "0048", "0050", "0054", "0080", "0218", "0219", "0220", "1902", "3101" };
            string[] mask04 = { "0051", "0055", "0298", "0317", "0333", "0334" };
            string[] maskR21 = { "0002", "0003", "0004", "0005", "0006", "0007", "0008", "0009", "0010", "0011", "0012", "0013", "0014", "0351" };
            string[] maskOther = { "0108", "0114", "0372" };

            if (maskType == "")
            {
                for (int i = 0; i < mask01.Length; i++)
                {
                    if (Convert.ToInt32(BookNo) == Convert.ToInt32(mask01[i])) maskType = "MaskType_1";
                }
            }
             if (maskType == "")
            {
                for (int i = 0; i < mask02.Length; i++)
                {
                    if (Convert.ToInt32(BookNo) == Convert.ToInt32(mask02[i])) maskType = "MaskType_2";
                }
            }
             if (maskType == "")
            {
                for (int i = 0; i < mask03.Length; i++)
                {
                    if (Convert.ToInt32(BookNo) == Convert.ToInt32(mask03[i])) maskType = "MaskType_3";
                }
            }
             if (maskType == "")
            {
                for (int i = 0; i < mask04.Length; i++)
                {
                    if (Convert.ToInt32(BookNo) == Convert.ToInt32(mask04[i])) maskType = "MaskType_4";
                }
            }
             if (maskType == "")
            {
                for (int i = 0; i < maskR21.Length; i++)
                {
                    if (Convert.ToInt32(BookNo) == Convert.ToInt32(maskR21[i])) maskType = "MaskType_5";
                }
            }
             if (maskType == "")
            {
                for (int i = 0; i < maskOther.Length; i++)
                {
                    if (Convert.ToInt32(BookNo) == Convert.ToInt32(maskOther[i])) maskType = "MaskType_6";
                }
            }
            if (maskType == "")
            {
                maskType = "MaskType_2";
            }

            return maskType;

        }
        public static byte[] getImgHide(Int32 BookNo, Int32 PageNo, Int32 Ver, string LineNo)
        {
           

            WorkSpace.Service WS = new WorkSpace.Service();
            NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
            NWS.Add(new NextwaverDB.NWhere("BOOKNO", BookNo.ToString()));
            NWS.Add(new NextwaverDB.NWhere("PAGENO", PageNo.ToString()));
            NWS.Add(new NextwaverDB.NWhere("PAGEVERSION", Ver.ToString()));

            NextwaverDB.NColumns NCS_S = new NextwaverDB.NColumns();
            NCS_S.Add(new NextwaverDB.NColumn("IMGURL"));

            string NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
            string NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);

            DataTable dt = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "Page", NCS_Encrypt, NWS_Encrypt, "System");

            string strpath = dt.Rows[0]["IMGURL"].ToString();

            try
            {
                MemoryStream ms = new MemoryStream();

                string ImageBack = "//" + strHostName + "/" + pathImgGun + strpath;

                //  string ImageFore = "//DESKTOP-O3I9FMD/Mask_Line/Line_1.png";

                string maskType =  ConfigMaskTarget(BookNo.ToString());

                string ImageFore = "//" + strHostName + "/" + pathMaskLine + maskType + "/R" + Convert.ToInt32(LineNo).ToString("00") + ".png";
                //string ImageFore = "//" + strHostName + "/" + pathMaskLine + "Line_" + LineNo + ".png";
                //-------------Resize Image---------------------------------
                MemoryStream ms02 = new MemoryStream();
                
                Image m2 = Image.FromFile(ImageBack);

                Image myImage = Image.FromFile(ImageBack);
                ResizeBilinear ResizeImage = new ResizeBilinear(3000, 4200);

                myImage = ResizeImage.Apply((Bitmap)myImage);
                m2 = ResizeImage.Apply((Bitmap)m2);

                Image imgF = Image.FromFile(ImageFore);
                imgF = ResizeImage.Apply((Bitmap)imgF);

                //----------------Protect Image---------------------------------

                System.Drawing.Graphics myGraphic = null;

                Image imgB;// =new Image.FromFile(ImageBack);
                imgB = Image.FromFile(ImageBack);

                //Image imgF;// =new Image.FromFile(ImageBack);
                //imgF = Image.FromFile(ImageFore);

                Image m;
                m = Image.FromFile(ImageBack);
                // 

                myGraphic = System.Drawing.Graphics.FromImage(myImage);
                myGraphic.DrawImageUnscaled(myImage, 0, 0);
                myGraphic.DrawImageUnscaled(imgF, 0, 0);
                myGraphic.Save();

                //   m.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);


                //---------Convert GrayScale Mode----
                // Turn image into gray scale image
                Image grayScaled = (Image)MakeGrayscale(new Bitmap(myImage));

                using (Bitmap newBitmap = new Bitmap(grayScaled))
                {
                    newBitmap.SetResolution(300, 300);
                    //      newBitmap.Save(result + "\\" + fileName, ImageFormat.Jpeg);

                    newBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                }
                //-------------------------------

                //   myImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                myImage.Dispose();





                return ms.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion
        static string ConcatZero(string str,int length)
        {
            int start = str.Length;
            for (int i = start; i < length; i++)
            {
                str = "0" + str;
            }
            return str;
        }
        public static byte[] getEditimg(Int32 BookNo, Int32 PageNo, Int32 VerNo)
        {
            string strSelectData = @" SELECT [ID]
                                          ,[BookNo]
                                          ,[BookYear]
                                          ,[GunRegIDPrefix]
                                          ,[GunRegIDStart]
                                          ,[GunRegIDEnd]
                                          ,[GunGroup]
                                          ,[PageTotal]
                                          ,[BookStatus]
                                          ,[CreateDate]
                                          ,[CreateBy]
                                          ,[UpdateDate]
                                          ,[UpdateBy]
                                      FROM [GunRegBook].[dbo].[Book]
                                      WHERE BookNo = " + BookNo;

            try
            {
                DataTable dt = GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);

                string iYear = "" + dt.Rows[0]["BookYear"];
                string iBookNo = "" + BookNo;
                int Index = iBookNo.Length;
                for (int i = Index; i < 4; i++)
                {
                    iBookNo = "0" + iBookNo;
                }

                string FolderType = "Edit";

                string root_path = "//" + strHostName + "/" + pathImgGun;
                //\\DESKTOP-J7DR4GS\imageBookGunReg
                string endFolder = root_path + FolderType + "/Y" + iYear + "/B" + iBookNo;

                string FileName = "B" + ConcatZero("" + BookNo, 4) + "_P" + ConcatZero("" + PageNo, 4) + "_V" + ConcatZero("" + VerNo, 2) + ".jpg";
                string strpath = endFolder + "/" + FileName;

                MemoryStream ms = new MemoryStream();
                Image img = Image.FromFile(strpath);
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                return ms.ToArray();
            }
            catch (Exception)
            {
                throw;
            }          
            
        }
        public static bool SaveEditImage(Int32 BookNo, Int32 PageNo, Int32 VerNo, byte[] bin)
        {
            string strSelectData = @" SELECT [ID]
                                          ,[BookNo]
                                          ,[BookYear]
                                          ,[GunRegIDPrefix]
                                          ,[GunRegIDStart]
                                          ,[GunRegIDEnd]
                                          ,[GunGroup]
                                          ,[PageTotal]
                                          ,[BookStatus]
                                          ,[CreateDate]
                                          ,[CreateBy]
                                          ,[UpdateDate]
                                          ,[UpdateBy]
                                      FROM [GunRegBook].[dbo].[Book]
                                      WHERE BookNo = " + BookNo;

            try
            {
                DataTable dt = GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);

                string iYear = "" + dt.Rows[0]["BookYear"];
                string iBookNo = "" + BookNo;
                int Index = iBookNo.Length;
                for (int i = Index; i < 4; i++)
                {
                    iBookNo = "0" + iBookNo;
                }

                string FolderType = "Edit";

                string root_path = "//" + strHostName + "/" + pathImgGun;
                //\\DESKTOP-J7DR4GS\imageBookGunReg
                string endFolder = root_path + FolderType + "/Y" + iYear + "/B" + iBookNo;

                string FileName = "B" + ConcatZero("" + BookNo, 4) + "_P" + ConcatZero(""+ PageNo, 4) + "_V" + ConcatZero(""+ VerNo, 2) + ".jpg";
                //B0122_P0016_V01.jpg

                if (!Directory.Exists(root_path + FolderType))
                    Directory.CreateDirectory(root_path + FolderType);
                if (!Directory.Exists(root_path + FolderType + "/Y" + iYear))
                    Directory.CreateDirectory(root_path + FolderType + "/Y" + iYear);
                if (!Directory.Exists(root_path + FolderType + "/Y" + iYear + "/B" + iBookNo))
                    Directory.CreateDirectory(root_path + FolderType + "/Y" + iYear + "/B" + iBookNo);

                File.WriteAllBytes(endFolder + "/" + FileName, bin);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static bool getDetailBook(Int32 BookNo,string FolderType)
        {
            string strSelectData = @" SELECT [ID]
                                          ,[BookNo]
                                          ,[BookYear]
                                          ,[GunRegIDPrefix]
                                          ,[GunRegIDStart]
                                          ,[GunRegIDEnd]
                                          ,[GunGroup]
                                          ,[PageTotal]
                                          ,[BookStatus]
                                          ,[CreateDate]
                                          ,[CreateBy]
                                          ,[UpdateDate]
                                          ,[UpdateBy]
                                      FROM [GunRegBook].[dbo].[Book]
                                      WHERE BookNo = " + BookNo;
            
            try
            {
                DataTable dt = GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);
                string iYear = "" + dt.Rows[0]["BookYear"];
                string iBookNo = "" + BookNo;
                int Index = iBookNo.Length;
                for (int i = Index; i < 4; i++)
                {
                    iBookNo = "0" + iBookNo;
                }

                string root_path = "//" + strHostName + "/" + pathImgGun;
                //\\DESKTOP-J7DR4GS\imageBookGunReg
                string startFolder = root_path + "Active/Y" + iYear + "/B" + iBookNo;
                string endFolder = root_path + FolderType + "/Y" + iYear + "/B" + iBookNo;


                if (!Directory.Exists(root_path + FolderType))
                    Directory.CreateDirectory(root_path + FolderType);
                if (!Directory.Exists(root_path + FolderType+"/Y"+ iYear))
                    Directory.CreateDirectory(root_path + FolderType + "/Y" + iYear);
                if (!Directory.Exists(root_path + FolderType + "/Y" + iYear+"/B"+ iBookNo))
                    Directory.CreateDirectory(root_path + FolderType + "/Y" + iYear + "/B" + iBookNo);

                DirectoryInfo dirTemp = new DirectoryInfo(startFolder);
                FileInfo[] fileList = dirTemp.GetFiles();
               
                for(int i = 0; i < fileList.Length; i++)
                {
                   
                    //System.IO.File.WriteAllText(endFolder + "/aaa.txt", "[" + fileList[i].Name + "." + fileList[i].Extension + "]");
                    File.Copy(fileList[i].FullName, endFolder + "/" + fileList[i].Name);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static bool getDetailBook_ByID(Int32 BookID, string FolderType)
        {
            string strSelectData = @" SELECT [ID]
                                          ,[BookNo]
                                          ,[BookYear]
                                          ,[GunRegIDPrefix]
                                          ,[GunRegIDStart]
                                          ,[GunRegIDEnd]
                                          ,[GunGroup]
                                          ,[PageTotal]
                                          ,[BookStatus]
                                          ,[CreateDate]
                                          ,[CreateBy]
                                          ,[UpdateDate]
                                          ,[UpdateBy]
                                      FROM [GunRegBook].[dbo].[Book]
                                      WHERE ID = " + BookID;

            try
            {
                DataTable dt = GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);
                string iYear = "" + dt.Rows[0]["BookYear"];
                string iBookNo = "" + dt.Rows[0]["BookNo"];
                int Index = iBookNo.Length;
                for (int i = Index; i < 4; i++)
                {
                    iBookNo = "0" + iBookNo;
                }

                string root_path = "//" + strHostName + "/" + pathImgGun;
                //\\DESKTOP-J7DR4GS\imageBookGunReg
                string startFolder = root_path + "Active/Y" + iYear + "/B" + iBookNo;
                string endFolder = root_path + FolderType + "/Y" + iYear + "/B" + iBookNo;


                if (!Directory.Exists(root_path + FolderType))
                    Directory.CreateDirectory(root_path + FolderType);
                if (!Directory.Exists(root_path + FolderType + "/Y" + iYear))
                    Directory.CreateDirectory(root_path + FolderType + "/Y" + iYear);
                if (!Directory.Exists(root_path + FolderType + "/Y" + iYear + "/B" + iBookNo))
                    Directory.CreateDirectory(root_path + FolderType + "/Y" + iYear + "/B" + iBookNo);

                DirectoryInfo dirTemp = new DirectoryInfo(startFolder);
                FileInfo[] fileList = dirTemp.GetFiles();
               
                for (int i = 0; i < fileList.Length; i++)
                {

                    //System.IO.File.WriteAllText(endFolder + "/aaa.txt", "[" + fileList[i].Name + "." + fileList[i].Extension + "]");
                    File.Copy(fileList[i].FullName, endFolder + "/" + fileList[i].Name);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static bool getReBook_ByID(Int32 BookID, string FolderType)
        {
            string strSelectData = @" SELECT [ID]
                                          ,[BookNo]
                                          ,[BookYear]
                                          ,[GunRegIDPrefix]
                                          ,[GunRegIDStart]
                                          ,[GunRegIDEnd]
                                          ,[GunGroup]
                                          ,[PageTotal]
                                          ,[BookStatus]
                                          ,[CreateDate]
                                          ,[CreateBy]
                                          ,[UpdateDate]
                                          ,[UpdateBy]
                                      FROM [GunRegBook].[dbo].[Book]
                                      WHERE ID = " + BookID;

            try
            {
                DataTable dt = GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);
                string iYear = "" + dt.Rows[0]["BookYear"];
                string iBookNo = "" + dt.Rows[0]["BookNo"];
                int Index = iBookNo.Length;
                for (int i = Index; i < 4; i++)
                {
                    iBookNo = "0" + iBookNo;
                }

                string root_path = "//" + strHostName + "/" + pathImgGun;
                //\\DESKTOP-J7DR4GS\imageBookGunReg
               
                string endFolder = root_path + FolderType + "/Y" + iYear + "/B" + iBookNo;
                //DirectoryInfo dirTemp = new DirectoryInfo(endFolder);
                //FileInfo[] fileList = dirTemp.GetFiles();
                //for (int i = 0; i < fileList.Length; i++)
                //{
                //    //System.IO.File.WriteAllText(endFolder + "/aaa.txt", "[" + fileList[i].Name + "." + fileList[i].Extension + "]");
                //    File.Delete(endFolder + "/" + fileList[i].Name + "." + fileList[i].Extension);
                //}

                Directory.Delete(endFolder, true);                   

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static bool deleteLog(string ID)
        {
            string strSelectData = @"DELETE [GunRegBook].[dbo].[Logs] WHERE ID = " + ID;
            try
            {
                return GRB_Dat.DBExecute.ExecuteTextNonQuery(strSelectData);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static DataTable getExpired_Book()
        {
            string Sql = @"SELECT *
                          FROM [GunRegBook].[dbo].[Book]
                          WHERE [Expire_Date] IS NOT NULL
                          AND CONVERT(Datetime, [Expire_Date]) < GETDATE()";
            try
            {
                return GRB_Dat.DBExecute.ExecuteTextQuery(Sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static DataTable getAllVersion(Int32 BookNo,Int32 PageNo)
        {
            //string Sql = @"SELECT [PageVersion]
            //              FROM [GunRegBook].[dbo].[Page]
            //              WHERE BookNo = @BookNo 
            //              AND [PageNo] = @PageNo";

            string Sql = @"SELECT [PageVersion],B.BookYear
                          FROM [GunRegBook].[dbo].[Page] P INNER JOIN [dbo].[Book] B ON P.BookNo = B.BookNo
                          WHERE P.BookNo = @BookNo 

                          AND [PageNo] = @PageNo";
            Sql = Sql.Replace("@PageNo", "" + PageNo);
            Sql = Sql.Replace("@BookNo", "" + BookNo);
            try
            {
                return GRB_Dat.DBExecute.ExecuteTextQuery(Sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region WaterMask
        public static bool SaveWaterMask(string ImgURL, byte[] imageByte)
        {
           
                MemoryStream stream = new MemoryStream(imageByte);

                Image myImage = Image.FromStream(stream);

                Boolean saveImgResult = ImageUtil.SaveImageWaterMark("//" + strHostName + "/" + pathWaterMask+ ImgURL, myImage);

               return true;      
       
        }

        public static byte[] getImgOfPrintWaterMask(Int32 BookNo, Int32 PageNo, Int32 Ver, string urlWaterMask)
        {

            WorkSpace.Service WS = new WorkSpace.Service();
            NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
            NWS.Add(new NextwaverDB.NWhere("BOOKNO", BookNo.ToString()));
            NWS.Add(new NextwaverDB.NWhere("PAGENO", PageNo.ToString()));
            NWS.Add(new NextwaverDB.NWhere("PAGEVERSION", Ver.ToString()));

            NextwaverDB.NColumns NCS_S = new NextwaverDB.NColumns();
            NCS_S.Add(new NextwaverDB.NColumn("IMGURL"));

            string NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
            string NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);

            DataTable dt = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "Page", NCS_Encrypt, NWS_Encrypt, "System");

            string strpath = dt.Rows[0]["IMGURL"].ToString();
            try
            {

                MemoryStream ms = new MemoryStream();

                string ImageBack = "//" + strHostName + "/" + pathImgGun + strpath;
                //  string ImageFore = "//DESKTOP-O3I9FMD/Mask_Line/Line_1.png";
          //     string ImageFore = "D:\\Mask_Line\\Line_" + LineNo + ".png"; 
              string ImageFore = "//" + strHostName + "/" + pathWaterMask + urlWaterMask;
                //-------------Resize Image---------------------------------
                MemoryStream ms02 = new MemoryStream();

                Image m2 = Image.FromFile(ImageBack);

                Image myImage = Image.FromFile(ImageBack);
                ResizeBilinear ResizeImage = new ResizeBilinear(3000, 4200);

                myImage = ResizeImage.Apply((Bitmap)myImage);
                m2 = ResizeImage.Apply((Bitmap)m2);

                Image imgF = Image.FromFile(ImageFore);
                imgF = ResizeImage.Apply((Bitmap)imgF);
                //-----------------------------
                Image imgB = Image.FromFile(ImageBack);
               // ResizeBilinear ResizeImage = new ResizeBilinear(3800, 5000);

                imgB = ResizeImage.Apply((Bitmap)imgB);
                //----------------Protect Image---------------------------------

                System.Drawing.Graphics myGraphic = null;

                //Image imgB;// =new Image.FromFile(ImageBack);
                //imgB = Image.FromFile(ImageBack);

                //Image imgF;// =new Image.FromFile(ImageBack);
                //imgF = Image.FromFile(ImageFore);
                Image m;
                m = Image.FromFile(ImageBack);
                // 

                myGraphic = System.Drawing.Graphics.FromImage(myImage);
                myGraphic.DrawImageUnscaled(myImage, 0, 0);
                myGraphic.DrawImageUnscaled(imgF, 0, 0);
                myGraphic.Save();

                //   m.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                myImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                myImage.Dispose();

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion

        #region DESTRORY
        public static DataTable getDestroryBook()
        {
            string Sql = @"SELECT *
                          FROM [GunRegBook].[dbo].[Book]
                          WHERE [Destrory_Date] IS NOT NULL
                          AND CONVERT(Datetime, [Destrory_Date]) < GETDATE()";
            try
            {
                return GRB_Dat.DBExecute.ExecuteTextQuery(Sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
          public static bool DeleteDestroryByID(Int32 BookID, string FolderType)
        {
            string strSelectData = @" SELECT [ID]
                                          ,[BookNo]
                                          ,[BookYear]
                                          ,[GunRegIDPrefix]
                                          ,[GunRegIDStart]
                                          ,[GunRegIDEnd]
                                          ,[GunGroup]
                                          ,[PageTotal]
                                          ,[BookStatus]
                                          ,[CreateDate]
                                          ,[CreateBy]
                                          ,[UpdateDate]
                                          ,[UpdateBy]
                                      FROM [GunRegBook].[dbo].[Book]
                                      WHERE ID = " + BookID;

            try
            {
                DataTable dt = GRB_Dat.DBExecute.ExecuteTextQuery(strSelectData);
                string iYear = "" + dt.Rows[0]["BookYear"];
                string iBookNo = "" + dt.Rows[0]["BookNo"];
                int Index = iBookNo.Length;
                for (int i = Index; i < 4; i++)
                {
                    iBookNo = "0" + iBookNo;
                }
                string root_path = "//" + strHostName + "/" + pathImgGun;
           
                string fullPath = root_path + FolderType + "/Y" + iYear + " /B" + iBookNo;

                Directory.Delete(fullPath, true);                   

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion



        #region setWebConfig
        public static string setWebConfig(string _imgPath)
        {
 
            Configuration config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);

            config.AppSettings.Settings.Remove("pathImg");
            config.AppSettings.Settings.Add("pathImg",_imgPath + "/");
            config.Save();

            return "OK";
        }
        public static string getDataWebConfig()
        {
            //----------------web management config-----------------------------
            Configuration config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);

            string _ParthImage_defult = WebConfigurationManager.AppSettings["pathImgGun"];
            return _ParthImage_defult;

        }
        #endregion



        public static Bitmap MakeGrayscale(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);
            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });
            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();
            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);
            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }
    }
}
