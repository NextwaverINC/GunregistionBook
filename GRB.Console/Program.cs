using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GRB.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].ToUpper() == "BACKUPDATA")
                {
                    System.Console.WriteLine("BackUp Data");
                    BackUpDoc.BackupDocument bk = new BackUpDoc.BackupDocument();
                    bk.BackupDocumentData(args);
                }
                else if (args[0].ToUpper() == "GENPDF")
                {
                    System.Console.WriteLine("Generate PDF");
                    GenImgToPdf.GenImgToPdf gp = new GenImgToPdf.GenImgToPdf();
                    gp.GenPdf(args);
                }
                else if (args[0].ToUpper() == "EXPIRED")
                {
                    System.Console.WriteLine("START EXPIRED");
                    Expired.Expired EX = new Expired.Expired();
                    try
                    {
                        EX.Start();
                    }catch(Exception ex){
                        System.Console.WriteLine("ERROR EXPIRED");
                        System.Console.WriteLine(ex.Message);
                    }
                    System.Console.WriteLine("END EXPIRED");

                }
                else if (args[0].ToUpper() == "DESTRORY")
                {
                    System.Console.WriteLine("START DESTRORY");
                    Destrory.Destrory DST = new Destrory.Destrory(); 
                    try
                    {
                        DST.Start();
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("ERROR DESTRORY");
                        System.Console.WriteLine(ex.Message);
                    }
                    System.Console.WriteLine("END DESTRORY");

                }
                else
                {
                    System.Console.WriteLine("Not Command");
                }
                 

            }
            else
            {
                System.Console.WriteLine("Not Command");
            }
        }
    }
}
