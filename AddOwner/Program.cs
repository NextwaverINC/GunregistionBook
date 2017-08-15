using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AddOwner
{
    class Program
    {
        static void Main(string[] args)
        {
            string Connection = "NextwaverDatabase";
            string OfficeSpaceId = "OF.0001";
            string DatabaseName = "GunBook";

            System.Collections.Generic.IEnumerable<String> lines = File.ReadLines("file.txt");
            int sIndex = 0;
            foreach (var item in lines)
            {
                try
                {
                    if (item.Trim() != "")
                    {
                        sIndex++;

                        WorkSpace.Service WS = new WorkSpace.Service();

                        NextwaverDB.NWheres NWS = new NextwaverDB.NWheres();
                        NWS.Add(new NextwaverDB.NWhere("NAME", item));

                        NextwaverDB.NColumns NCS_S = new NextwaverDB.NColumns();
                        NCS_S.Add(new NextwaverDB.NColumn("NAME"));

                        string NCS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NCS_S.ExportString(), true);
                        string NWS_Encrypt = new EncryptDecrypt.CryptorEngine().Encrypt(NWS.ExportString(), true);

                        DataTable dt = WS.SelectByColumnAndWhere(Connection, OfficeSpaceId, DatabaseName, "GunOwner", NCS_Encrypt, NWS_Encrypt, "System");

                        if (dt.Rows.Count == 0)
                        {
                            try
                            {
                                NextwaverDB.NColumns NCS = new NextwaverDB.NColumns();
                                NCS.Add(new NextwaverDB.NColumn("NAME", item));
                                NCS.Add(new NextwaverDB.NColumn("DESCRIPTION", ""));
                                NCS.Add(new NextwaverDB.NColumn("UPDATEDATE", convertDatetime(DateTime.Now)));
                                NCS.Add(new NextwaverDB.NColumn("DELETE", "0"));
                                string[] OP = WS.InsertData(Connection, OfficeSpaceId, DatabaseName, "GunOwner", NCS.ExportString(), "", "System");

                                if (OP[0].ToUpper() == "OK")
                                {
                                    System.Console.WriteLine(sIndex + " :: Add :: " + item);
                                }
                                else
                                {
                                    System.Console.WriteLine("Add :: " + item + " fail");
                                }
                            }
                            catch (Exception ex)
                            {

                            }


                        }
                        else
                            System.Console.WriteLine("Have :: " + item);
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }

        static string convertDatetime(DateTime date)
        {
            int yearTh = date.Year;
            if (yearTh < 2400)
                yearTh = yearTh + 543;

            return date.Day.ToString() + "/" + date.Month.ToString() + "/" + yearTh.ToString() + " " + date.Hour.ToString() + ":" + date.Minute.ToString() + ":" + date.Second.ToString();
        }

    }
}
