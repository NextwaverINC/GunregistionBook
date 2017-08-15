using System;
using System.Data;
using GRB_Dat = GRB.DataAccess;
using System.Xml;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Drawing;
using GRB.Business.Logic;

namespace GRB.Business.Logic
{
    public class GunRegistration
    {
        static string Connection = "NextwaverDatabase";
        static string OfficeSpaceId = "OF.0001";
        static string DatabaseName = "GunBook";
        static string pathImgGun = ConfigurationManager.AppSettings["pathImgGun"];
        static string pathImgGunReplace = ConfigurationManager.AppSettings["pathImgGunReplace"];
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
                xDocPage.Load(PathXml);

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
    , string GunOwner, string GunRegDate, string GunRemark, string GunCountry)
        {
            try
            {
                return GRB_Dat.spGBGetTblGunRegistration.ExecuteReader(BookNo, PageNo, PageVersion, GunRegID, GunNo
    , GunGroup, GunType, GunSize, GunBrand, GunMaxShot, GunBarrel, GunColor
    , GunOwner, GunRegDate, GunRemark, GunCountry);
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
            WorkSpace.Service WS = new WorkSpace.Service();

            NextwaverDB.NColumns NCS = new NextwaverDB.NColumns();

            string strResult = "Unsuccess";
            if (ProcessResult)
                strResult = "success";

            NCS.Add(new NextwaverDB.NColumn("USERNAME", UserName));
            NCS.Add(new NextwaverDB.NColumn("MENU", MenuUse));
            NCS.Add(new NextwaverDB.NColumn("PROCESS", UserProcess));
            NCS.Add(new NextwaverDB.NColumn("RESULT", strResult));
            NCS.Add(new NextwaverDB.NColumn("PARAM", paramsList));
            NCS.Add(new NextwaverDB.NColumn("DATETIME", DateTime.Now.ToString("F")));

            string[] OP = WS.InsertData(Connection, OfficeSpaceId, DatabaseName, "LogUser", NCS.ExportString(), "", "System");
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

    }
}
