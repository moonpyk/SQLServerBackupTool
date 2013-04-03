using System.Collections.Generic;
using System.Linq;
using SQLServerBackupTool.Web.Models;

namespace SQLServerBackupTool.Web.ViewModels
{
    public class IndexViewModel
    {
        public IndexViewModel(IEnumerable<DatabaseInfo> d, IEnumerable<BackupHistory> b)
        {
            DatabaseInfo = d;
            Backups = b.ToList();
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