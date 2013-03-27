using System.Collections.Generic;
using SQLServerBackupTool.Web.Models;

namespace SQLServerBackupTool.Web.ViewModels
{
    public class IndexViewModel
    {
        public IndexViewModel(IEnumerable<DatabaseInfo> d, IEnumerable<BackupHistory> b)
        {
            DatabaseInfo = d;
            Backups = b;
        }

        public IEnumerable<BackupHistory> Backups
        {
            get;
            set;
        }

        public IEnumerable<DatabaseInfo> DatabaseInfo
        {
            get;
            set;
        }
    }
}