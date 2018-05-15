using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using GRB_BizLog = GRB.Business.Logic;
using System.Drawing;
using System.Diagnostics;

namespace GRB.WebService
{
    /// <summary>
    /// Summary description for GRB_WebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class GRB_WebService : System.Web.Services.WebService
    {
        //
        [WebMethod]
        public DataTable GetAllVersion(Int32 BookNo, Int32 PageNo)
        {
            return GRB_BizLog.GunRegistration.getAllVersion(BookNo, PageNo);
        }

        [WebMethod]
        public DataTable GetBook(Int32 BookNo)
        {
            return GRB_BizLog.GunRegistration.GetBook(BookNo);
        }

        [WebMethod]
        public DataTable GetBookByCondition(Int32 BookNo, Int32 BookYear)
        {
            return GRB_BizLog.GunRegistration.GetBookByCondition(BookNo, BookYear);
        }

        [WebMethod]
        public DataTable GetPage(Int32 BookNo, Int32 PageNo)
        {
            return GRB_BizLog.GunRegistration.GetPage(BookNo, PageNo);
        }

        [WebMethod]
        public DataTable GetPageVersion(Int32 BookNo, Int32 PageNo, Int32 Version)
        {
            return GRB_BizLog.GunRegistration.GetPageVer(BookNo, PageNo, Version);
        }

        [WebMethod]
        public bool SaveImagePage(Int32 BookNo, Int32 PageNo, Int32 Version, string ImgURL, string UserName, byte[] imageByte)
        {
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

            String PathXml = Server.MapPath("/tempdoc/Page.xml");
            if (!System.IO.File.Exists(PathXml))
                PathXml = Server.MapPath("/GRBServices_ws/tempdoc/Page.xml");

            Boolean isSuccess = false;
            Exception err = new Exception();
            for (int index = 0; index < 5; index++)
            {
                try
                {
                    GRB_BizLog.GunRegistration.SaveImagePage(BookNo, PageNo, Version, ImgURL, PathXml, UserName, imageByte);
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    err = ex;
                    isSuccess = false;
                }
                if (isSuccess)
                    break;
                else
                {
                    System.Threading.Thread.Sleep(500);
                }

            }
            if (!isSuccess)
                throw new Exception("Error", err);

            return true;
        }

        [WebMethod]
        public DataTable UserAuth(string UserName, string Password)
        {
            return GRB_BizLog.GunRegistration.UserAuth(UserName, Password);
        }

        [WebMethod]
        public byte[] getimg(string pathImg)
        {
            return GRB_BizLog.GunRegistration.getimg(pathImg);
        }

        [WebMethod]
        public byte[] GetCurrentImagePage(Int32 BookNo, Int32 PageNo)
        {
            return GRB_BizLog.GunRegistration.getimg(BookNo, PageNo);
        }
        //getEditimg
        [WebMethod]
        public byte[] GetImagePage(Int32 BookNo, Int32 PageNo, Int32 PageVer)
        {
            return GRB_BizLog.GunRegistration.getimg(BookNo, PageNo, PageVer);
        }
        [WebMethod]
        public byte[] GetEditImagePage(Int32 BookNo, Int32 PageNo, Int32 PageVer)
        {
            return GRB_BizLog.GunRegistration.getEditimg(BookNo, PageNo, PageVer);
        }

        [WebMethod]
        public string GetImageUrl(Int32 BookNo, Int32 PageNo, Int32 PageVer)
        {
            return GRB_BizLog.GunRegistration.getimgURL(BookNo, PageNo, PageVer);
        }

        public bool EditPathImg(int bookno)
        {
            return GRB_BizLog.GunRegistration.EditPageImg(bookno);
        }

        [WebMethod]
        public DataTable GetTblGunReg(Int32 BookNo, Int32 PageNo, Int32 PageVersion, string GunRegID, string GunNo
        , string GunGroup, string GunType, string GunSize, string GunBrand, Int32 GunMaxShot, string GunBarrel, string GunColor
        , string GunOwner, string GunRegDate, string GunRemark, string GunCountry, int GunIdOnly)
        {
            return GRB_BizLog.GunRegistration.GetTblGunReg(BookNo, PageNo, PageVersion, GunRegID, GunNo
    , GunGroup, GunType, GunSize, GunBrand, GunMaxShot, GunBarrel, GunColor
    , GunOwner, GunRegDate, GunRemark, GunCountry, GunIdOnly);
        }

        [WebMethod]
        public DataTable GetDataGun(string DateBegin, string DateEnd, string pStatus)
        {
            return GRB_BizLog.GunRegistration.GetDataGun(DateBegin, DateEnd, pStatus);
        }

        [WebMethod]
        public DataTable GetDataGunSum(string DateBegin, string DateEnd, string pStatus)
        {
            return GRB_BizLog.GunRegistration.GetDataGunSum(DateBegin, DateEnd, pStatus);
        }

        [WebMethod]
        public bool RunBatBackupfile(int bookno)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo(@"C:\test\mybatch.bat");
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.Arguments = "";
                Process process = Process.Start(processInfo);
                //process.WaitForExit();
                //if (process.ExitCode == 0)
                //{ // success

                //}
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [WebMethod]
        public void logUser(string UserName, string MenuUse, string UserProcess, bool ProcessResult, string strList)
        {
            try
            {
                GRB_BizLog.GunRegistration.logUserGun(UserName, MenuUse, UserProcess, ProcessResult, strList);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [WebMethod]
        public byte[] getPDF(string pathPDF)
        {
            return GRB_BizLog.GunRegistration.getPdf(pathPDF);
        }

        [WebMethod]
        public bool updatePercentErrImg(Int32 BookNo, Int32 PageNo, decimal PercentErr)
        {
            return GRB_BizLog.GunRegistration.updatePercentErr(BookNo, PageNo, PercentErr);
        }

        [WebMethod]
        public bool insertLog(string UserName, string Pocess, string Detail, string DateUpdate)
        {
            return GRB_BizLog.GunRegistration.insertLog(UserName, Pocess, Detail, DateUpdate);
        }

        [WebMethod]
        public bool insertImg(Int32 BookNo, Int32 PageNo, string Detail)
        {
            return GRB_BizLog.GunRegistration.insertImg(BookNo, PageNo, Detail);
        }

        [WebMethod]
        public DataTable reportImgErr()
        {
            return GRB_BizLog.GunRegistration.reportImgErr();
        }

        [WebMethod]
        public DataTable reportLog()
        {
            return GRB_BizLog.GunRegistration.reportLog();
        }

        [WebMethod]
        public DataTable countAllData()
        {
            return GRB_BizLog.GunRegistration.countAllData();
        }

        [WebMethod]
        public DataTable countAllDataByYear()
        {
            return GRB_BizLog.GunRegistration.countAllDataByYear();
        }

        [WebMethod]
        public DataTable countAllDataByBook()
        {
            return GRB_BizLog.GunRegistration.countAllDataByBook();
        }

        #region GetImageHide
        [WebMethod]
        public DataTable GetDetailPage(string BookNo, string PageNo, string PageVer)
        {
            return GRB_BizLog.GunRegistration.getDetailPage(BookNo, PageNo, PageVer);
        }

        [WebMethod]
        public byte[] getImgHide(Int32 BookNo, Int32 PageNo, Int32 PageVer, string LineNo)
        {
            return GRB_BizLog.GunRegistration.getImgHide(BookNo, PageNo, PageVer, LineNo);
        }
        #endregion

        [WebMethod]
        public bool SaveEditImage(Int32 BookNo, Int32 PageNo, Int32 VerNo, byte[] bin)
        {
            return GRB_BizLog.GunRegistration.SaveEditImage(BookNo, PageNo, VerNo, bin);
        }
        //SaveEditImage(Int32 BookNo, Int32 PageNo, Int32 VerNo, byte[] bin)

        [WebMethod]
        public bool MoveToFolder(Int32 BookNo,string FolderType)
        {
            return GRB_BizLog.GunRegistration.getDetailBook(BookNo, FolderType);
        }

        [WebMethod]
        public bool MoveToFolderByID(Int32 ID, string FolderType)
        {
            return GRB_BizLog.GunRegistration.getDetailBook_ByID(ID, FolderType);
        }

        [WebMethod]
        public bool DeleteFolderByID(Int32 ID, string FolderType)
        {
            return GRB_BizLog.GunRegistration.getReBook_ByID(ID, FolderType);
        }
        [WebMethod]
        public bool DeleteLogByID(string ID)
        {
            return GRB_BizLog.GunRegistration.deleteLog(ID);
        }

        [WebMethod]
        public DataTable getExpiredBook()
        {
            return GRB_BizLog.GunRegistration.getExpired_Book();
        }

        #region SaveWaterMask
        [WebMethod]
        public bool SaveWaterMask(string ImgURL, byte[] imageByte)
        {

            GRB_BizLog.GunRegistration.SaveWaterMask(ImgURL, imageByte);
            return true;
        }
        [WebMethod]
        public byte[] getImgOfPrintWaterMask(Int32 BookNo, Int32 PageNo, Int32 PageVer, string urlWaterMask)
        {
            return GRB_BizLog.GunRegistration.getImgOfPrintWaterMask(BookNo, PageNo, PageVer, urlWaterMask);
        }
        #endregion

        #region API
        [WebMethod]
        public DataTable API_getRecord(string GunRegID)
        {
            return GRB_BizLog.GunRegistration.API_getRecord(GunRegID);
        }
        #endregion

        #region DESTRORY
        [WebMethod]
        public DataTable getDestroryBook()
        {
            return GRB_BizLog.GunRegistration.getDestroryBook();
        }


        [WebMethod]
        public bool DeleteDestroryByID(Int32 ID, string FolderType)
        {
            return GRB_BizLog.GunRegistration.DeleteDestroryByID(ID, FolderType);
        }
        #endregion


        #region WebConfig
        [WebMethod]
        public string setWebConfig(string _imgPath)
        {
            return GRB_BizLog.GunRegistration.setWebConfig(_imgPath);
        }
        [WebMethod]
        public string getDataWebConfig()
        {
            return GRB_BizLog.GunRegistration.getDataWebConfig();
        }
        #endregion
    }
}
