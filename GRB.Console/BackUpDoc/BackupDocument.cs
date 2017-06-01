using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace GRB.Console.BackUpDoc
{
    class BackupDocument
    {
        public void BackupDocumentData(string[] args)
        {
            try
            {
                string sourceDir = args[1];
                string backupDir = args[2];

                string fileLog = args[3]; //@"\logBack\Up\logDateBackup.txt";

                int position = fileLog.LastIndexOf('\\');

                string pahtlog = fileLog.Substring(0, position);
                if (!System.IO.Directory.Exists(pahtlog))
                    System.IO.Directory.CreateDirectory(pahtlog);

                if (!File.Exists(fileLog))
                {
                    File.WriteAllText(fileLog, "0");
                }

                string lastBackUp = File.ReadLines(fileLog).Last();

                foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
                {
                    if (!System.IO.Directory.Exists(dirPath.Replace(sourceDir, backupDir)))
                        System.IO.Directory.CreateDirectory(dirPath.Replace(sourceDir, backupDir));

                    DirectoryInfo dir = new DirectoryInfo(dirPath);
                    FileInfo[] fis = dir.GetFiles("*", SearchOption.TopDirectoryOnly);

                    foreach (FileInfo fi in fis)
                    {
                        string datefile = fi.LastWriteTime.ToString("yyyyMMddHH");

                        if (lastBackUp.Trim() == "0")
                            datefile = "0";

                        if (int.Parse(datefile.Trim()) >= int.Parse(lastBackUp.Trim()))
                            File.Copy(fi.FullName, dirPath.Replace(sourceDir, backupDir) + "\\" + fi.Name, true);
                    }
                }
                File.AppendAllText(fileLog, Environment.NewLine + DateTime.Now.ToString("yyyyMMddHH"));
            }
            catch (DirectoryNotFoundException dirNotFound)
            {
                System.Console.WriteLine(dirNotFound.Message);

                File.AppendAllText("c:\\Log\\BackUpDatalog.txt", Environment.NewLine + Environment.NewLine
                    + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " :: " + dirNotFound.Message);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);

                File.AppendAllText("c:\\Log\\BackUpDatalog.txt", Environment.NewLine + Environment.NewLine
                    + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " :: " + ex.Message);
            }


        }
    }
}
