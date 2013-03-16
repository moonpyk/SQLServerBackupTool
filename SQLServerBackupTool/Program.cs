using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Ionic.Zip;
using SQLServerBackupTool.Lib;
using SQLServerBackupTool.Properties;

namespace SQLServerBackupTool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Intro();

            var s = Settings.Default;
            using (var bk = new SqlServerBackupProvider(s.BackupConnection))
            {
                try
                {
                    Log(2, OutputStatusType.Info, "Opening connection...");

                    bk.Open();
                    Log(4, OutputStatusType.OK, "Database connection opened");
                }
                catch (Exception ex)
                {
                    LogException(ex);
                    Log(4, OutputStatusType.Error, "Error while opening database connection !");
                    Exit(false);
                    return;
                }

                foreach (var ddb in s.DatabaseList)
                {
                    Log(4, OutputStatusType.Info, string.Format("Doing database backup of '{0}'", ddb));

                    var ts = DateTime.Now;

                    var fNameBase = string.Format(
                        "{0}.{1}",
                        ddb,
                        ts.ToString("yyyyMMdd.HHmmss")
                        );

                    var backupFullPath = Path.Combine(
                        s.BackupPath,
                        string.Format("{0}.bak", fNameBase)
                        );

                    Log(4, OutputStatusType.Info, string.Format("Output file path : '{0}'", backupFullPath));

                    try
                    {
                        Log(7, OutputStatusType.Info, "Staring backup...");

                        bk.BackupDatabase(ddb, backupFullPath, ts);

                        Log(9, OutputStatusType.OK, "Backup done.");
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                        Log(9, OutputStatusType.Error, "Error while doing backup");
                        continue;
                    }

                    if (!File.Exists(backupFullPath))
                    {
                        Log(7, OutputStatusType.Warning,
                            "Unable to read the backup file, maybe the backup has been done on a remote server ?");
                        continue;
                    }

                    var toUpload = backupFullPath;

                    if (s.EnableBackupZip)
                    {
                        try
                        {
                            Log(7, OutputStatusType.Info, "Doing backup zip...");
                            using (var z = new ZipFile())
                            {
                                var zipFilePath = Path.Combine(s.BackupPath, string.Format("{0}.zip", fNameBase));
                                z.AddFile(backupFullPath, string.Empty);
                                z.Save(zipFilePath);
                            }
                            Log(9, OutputStatusType.OK, "Zip archive successfully created");
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                            Log(9, OutputStatusType.Error, "Error during zip archive creation");
                            continue;
                        }

                        if (s.EnableDeleteBackupAfterZip)
                        {
                            try
                            {
                                Log(7, OutputStatusType.Info, "Deleting original backup file...");
                                File.Delete(backupFullPath);
                                Log(9, OutputStatusType.OK, "Original backup file deleted.");
                            }
                            catch (Exception ex)
                            {
                                LogException(ex);
                                Log(9, OutputStatusType.Error, "Unable to delete original backup file.");
                            }
                        }
                    }
                }

                Log(2, OutputStatusType.Info, "All work is done, exiting.");

                Exit(true);
            }
        }

        private static void Intro()
        {
            var v = Assembly.GetCallingAssembly().GetName().Version;

            Console.Write("SQLServerBackupTool ");
            ConsoleHelper.WriteColor(ConsoleColor.Magenta, string.Format("v{0}", v));
            Console.Write(" by @moonpyk");
            Console.WriteLine();
        }

        private static void Exit(bool isClean)
        {
#if DEBUG
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
#endif
        }

        private static void Log(int indent, OutputStatusType s, string text)
        {
            ConsoleHelper.WriteStatus(indent, s, text);
        }

        private static void LogException(Exception ex)
        {
#if DEBUG
            Debug.WriteLine(ex);
#endif
        }
    }
}
