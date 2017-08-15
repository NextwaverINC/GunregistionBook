using System;
using System.Data;
using System.Data.SqlClient;
using GRB_Dat = GRB.DataAccess;

namespace GRB.DataAccess
{
    public class spGBGetTblGunRegistration
    {
        private const string _Procedure_Name = "dbo.spGBGetTblGunRegistration";

        public static DataTable ExecuteReader(Int32 BookNo, Int32 PageNo, Int32 PageVersion, string GunRegID, string GunNo
    , string GunGroup, string GunType, string GunSize, string GunBrand, Int32 GunMaxShot, string GunBarrel, string GunColor
    , string GunOwner, string GunRegDate, string GunRemark, string GunCountry)
        {
            SqlParameter Param_BookNo = new SqlParameter("@BookNo", BookNo);
            SqlParameter Param_PageNo = new SqlParameter("@PageNo", PageNo);
            SqlParameter Param_PageVersion = new SqlParameter("@PageVersion", PageVersion);

            SqlParameter Param_GunRegID = new SqlParameter("@GunRegID", GunRegID);
            SqlParameter Param_GunNo = new SqlParameter("@GunNo", GunNo);
            SqlParameter Param_GunGroup = new SqlParameter("@GunGroup", GunGroup);
            SqlParameter Param_GunType = new SqlParameter("@GunType", GunType);
            SqlParameter Param_GunSize = new SqlParameter("@GunSize", GunSize);
            SqlParameter Param_GunBrand = new SqlParameter("@GunBrand", GunBrand);
            SqlParameter Param_GunMaxShot = new SqlParameter("@GunMaxShot", GunMaxShot);
            SqlParameter Param_GunBarrel = new SqlParameter("@GunBarrel", GunBarrel);
            SqlParameter Param_GunColor = new SqlParameter("@GunColor", GunColor);
            SqlParameter Param_GunOwner = new SqlParameter("@GunOwner", GunOwner);
            SqlParameter Param_GunRegDate = new SqlParameter("@GunRegDate", GunRegDate);
            SqlParameter Param_GunRemark = new SqlParameter("@GunRemark", GunRemark);
            SqlParameter Param_GunCountry = new SqlParameter("@GunCountry", GunCountry);

            return (DataTable)GRB_Dat.DBExecute.ExecuteProcedureReader(_Procedure_Name, false, Param_BookNo, Param_PageNo, Param_PageVersion
                , Param_GunRegID, Param_GunNo, Param_GunGroup, Param_GunType, Param_GunSize, Param_GunBrand, Param_GunMaxShot, Param_GunBarrel
                , Param_GunColor, Param_GunOwner, Param_GunRegDate, Param_GunRemark, Param_GunCountry);
        }
    }
}
