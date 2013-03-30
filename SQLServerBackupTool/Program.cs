using System.Collections.Generic;
using Ionic.Zip;
using Mono.Options;
using SQLServerBackupTool.Lib;
using SQLServerBackupTool.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SQLServerBackupTool
{
    internal class Program
    {
        private static readonly List<string> DatabaseList = new List<string>();

        private static bool Silent
        {
            get;
            set;
        }

        private static bool ReadLine
        {
            get;
            set;
        }

        private static void Main(string[] args)
        {
            var showHelp = false;

            var opts = new OptionSet
            {
                {"h|?|help", "Displays this help message", _ => showHelp = _ != null},
                {
                    "s|silent",
                    "Put ssbt in silent mode, nothing will be written in the standard output (excludes --readline)",
                    _ => Silent = _ != null
                },
                {
                    "R|readline", "Don't exit directly after finish, wait for a key to be typed", _ => ReadLine = _ != null
                },
                {
                    "d|databases=", "Comma separated list of databases names to backup, overrides App.config databases list", delegate(string _)
                    {
                        if (!string.IsNullOrWhiteSpace(_))
                        {
                            DatabaseList.AddRange(_.Split(','));
                        }
                    }
                },
            };

            opts.Parse(args);

            Intro();

            if (showHelp)
            {
                Console.WriteLine();
                Console.WriteLine(" Usage :");
                opts.WriteOptionDescriptions(Console.Out);
                Exit(true);
                return;
            }

            var s = Settings.Default;

            SqlServerBackupProvider bk;

            try
            {
                bk = new SqlServerBackupProvider(s.BackupConnection);
            }
            catch (Exception ex)
            {
                Log(1, OutputStatusType.Error, "Something went wrong during SQL connection creation, you should check your connection string.");
                LogException(ex);
                Exit(false);
                return;
            }

            using (bk)
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

                // Nothing populated from command line
                if (DatabaseList.Count == 0)
                {
                    foreach (var confd in s.DatabaseList)
                    {
                        DatabaseList.Add(confd);
                    }
                }

                foreach (var ddb in DatabaseList)
                {
                    Log(4, OutputStatusType.Info, string.Format("Doing database backup of '{0}'", ddb));

                    var ts = DateTime.Now;

                    var fNameBase = Utils.GenerateBackupBaseName(ddb, ts);

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
            if (Silent)
            {
                return;
            }

            var v = Assembly.GetCallingAssembly().GetName().Version;

            Console.Write("SQLServerBackupTool ");
            ConsoleHelper.WriteColor(ConsoleColor.Magenta, string.Format("v{0}", v));
            Console.Write(" by @moonpyk");
            Console.WriteLine();
        }

        private static void Exit(bool isClean)
        {
            if (ReadLine && !Silent)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
            }

            Environment.ExitCode = !isClean
                ? 1
                : 0;
        }

        private static void Log(int indent, OutputStatusType s, string text)
        {
            if (Silent)
            {
                return;
            }
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
