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
        [WebMethod]
        public DataTable GetBook(Int32 BookNo)
        {
            return GRB_BizLog.GunRegistration.GetBook(BookNo);
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
            String PathXml = Server.MapPath("/tempdoc/Page.xml");
            if (!System.IO.File.Exists(PathXml))
                PathXml = Server.MapPath("/GRBServices_ws/tempdoc/Page.xml");
            return GRB_BizLog.GunRegistration.SaveImagePage(BookNo, PageNo, Version, ImgURL, PathXml, UserName, imageByte);
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

        [WebMethod]
        public byte[] GetImagePage(Int32 BookNo, Int32 PageNo, Int32 PageVer)
        {
            return GRB_BizLog.GunRegistration.getimg(BookNo, PageNo, PageVer);
        }

        [WebMethod]
        public bool EditPathImg(int bookno)
        {
            return GRB_BizLog.GunRegistration.EditPageImg(bookno);
        }

        [WebMethod]
        public DataTable GetTblGunReg(Int32 BookNo, Int32 PageNo, Int32 PageVersion, string GunRegID, string GunNo
        , string GunGroup, string GunType, string GunSize, string GunBrand, Int32 GunMaxShot, string GunBarrel, string GunColor
        , string GunOwner, string GunRegDate)
        {
            return GRB_BizLog.GunRegistration.GetTblGunReg( BookNo,  PageNo,  PageVersion,  GunRegID,  GunNo
    ,  GunGroup,  GunType,  GunSize,  GunBrand,  GunMaxShot,  GunBarrel,  GunColor
    ,  GunOwner,  GunRegDate);
        }

        [WebMethod]
        public bool RunBatBackupfile(int bookno)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo(@"C:\test\mybatch.bat");
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                Process process = Process.Start(processInfo);
                //process.WaitForExit();
                //if (process.ExitCode == 0)
                //{ // success

                //}
                return true;
            }
            catch(Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}
