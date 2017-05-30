﻿using System;
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
            //if (args.Length == 0)
            //{
            //    args = new string[3];
            //    args[0] = "BACKUPDATA";
            //    args[1] = System.Configuration.ConfigurationSettings.AppSettings["sourceDir"];
            //    args[2] = System.Configuration.ConfigurationSettings.AppSettings["backupDir"];
            //}
            if (args.Length > 0)
            {
                if (args[0].ToUpper() == "BACKUPDATA")
                {
                    System.Console.WriteLine("BackUp Data");
                    BackUpDoc.BackupDocument bk = new BackUpDoc.BackupDocument();
                    bk.BackupDocumentData(args);
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
