using System;
using System.Data.SqlClient;
using System.IO;
using SQLServerBackupTool.Properties;

namespace SQLServerBackupTool
{
    class Program
    {
        public const string BackupCommandTemplate = @"
BACKUP DATABASE [{0}] 
TO  DISK = N'{1}' 
WITH NOFORMAT, NOINIT,  NAME = N'{0} - {2}', SKIP, NOREWIND, NOUNLOAD,  STATS = 10;
";
        static void Main(string[] args)
        {
            var s = Settings.Default;
            using (var co = new SqlConnection(s.BackupConnection))
            {
                try
                {
                    ConsoleHelper.WriteStatus(2, OutputStatusType.Info, "Opening connection...");
                    ConsoleHelper.WriteStatus(4, OutputStatusType.OK, "Database connection opened");
                }
                catch (Exception)
                {
                    ConsoleHelper.WriteStatus(4, OutputStatusType.Error, "Error while opening database connection !");
                    return;
                }
                co.Open();

                foreach (var ddb in s.DatabaseList)
                {
                    ConsoleHelper.WriteStatus(4, OutputStatusType.Info, string.Format("Doing database backup of '{0}'", ddb));

                    var q = co.CreateCommand();
                    var now = DateTime.Now;

                    var backupPath = Path.Combine(s.BackupPath, string.Format("{0}.{1}.bak",
                        ddb,
                        now.ToString("yyyyMMdd.HHmmss")
                    ));

                    ConsoleHelper.WriteStatus(4, OutputStatusType.Info, string.Format("Output file path : '{0}'", backupPath));

                    q.CommandText = string.Format(BackupCommandTemplate,
                        ddb,
                        backupPath,
                        string.Format("{0} {1}", now.ToShortDateString(), now.ToShortTimeString())
                    );

                    try
                    {
                        ConsoleHelper.WriteStatus(7, OutputStatusType.Info, "Staring backup...");
                        q.ExecuteNonQuery();

                        ConsoleHelper.WriteStatus(9, OutputStatusType.OK, "Backup done.");
                    }
                    catch (Exception)
                    {
                        ConsoleHelper.WriteStatus(9, OutputStatusType.Error, "Error while doing backup");
                    }
                }

                ConsoleHelper.WriteStatus(2, OutputStatusType.Info, "All work is done, exiting.");

                Console.ReadLine();
            }
        }
    }
}
