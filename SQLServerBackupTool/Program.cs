using System;
using System.Data.SqlClient;
using System.IO;
using Ionic.Zip;
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

                    var fNameBase = string.Format(
                        "{0}.{1}",
                        ddb,
                        now.ToString("yyyyMMdd.HHmmss")
                    );

                    var backupFullPath = Path.Combine(
                        s.BackupPath,
                        string.Format("{0}.bak", fNameBase)
                    );

                    ConsoleHelper.WriteStatus(4, OutputStatusType.Info, string.Format("Output file path : '{0}'", backupFullPath));

                    q.CommandText = string.Format(BackupCommandTemplate,
                        ddb,
                        backupFullPath,
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
                        continue;
                    }

                    if (!File.Exists(backupFullPath))
                    {
                        ConsoleHelper.WriteStatus(7, OutputStatusType.Warning, "Unable to read the backup file, maybe the backup has been done on a remote server ?");
                        continue;
                    }

                    var toUpload = backupFullPath;

                    if (s.EnableBackupZip)
                    {
                        try
                        {
                            ConsoleHelper.WriteStatus(7, OutputStatusType.Info, "Doing backup zip...");
                            using (var z = new ZipFile())
                            {
                                var zipFilePath = Path.Combine(s.BackupPath, string.Format("{0}.zip", fNameBase));
                                z.AddFile(backupFullPath, string.Empty);
                                z.Save(zipFilePath);
                            }
                            ConsoleHelper.WriteStatus(9, OutputStatusType.OK, "Zip archive successfully created");
                        }
                        catch (Exception)
                        {
                            ConsoleHelper.WriteStatus(9, OutputStatusType.Error, "Error during zip archive creation");
                            continue;
                        }

                        if (s.EnableDeleteBackup)
                        {
                            try
                            {
                                ConsoleHelper.WriteStatus(7, OutputStatusType.Info, "Deleting original backup file...");
                                File.Delete(backupFullPath);
                                ConsoleHelper.WriteStatus(9, OutputStatusType.OK, "Original backup file deleted.");
                            }
                            catch (Exception)
                            {
                                ConsoleHelper.WriteStatus(9, OutputStatusType.Error, "Unable to delete original backup file.");
                            }
                        }
                    }
                }

                ConsoleHelper.WriteStatus(2, OutputStatusType.Info, "All work is done, exiting.");

                Console.ReadLine();
            }
        }
    }
}
