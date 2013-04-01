using SQLServerBackupTool.Lib.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Security;
using SQLServerBackupTool.Web.Lib;

namespace SQLServerBackupTool.Web.ViewModels
{
    public class MembershipEditViewModel
    {
        private static readonly List<string> _availableRoles;

        static MembershipEditViewModel()
        {
            _availableRoles = System.Web.Security.Roles.Enabled 
                ? System.Web.Security.Roles.GetAllRoles().ToList() 
                : new List<string>(0);
        }

        [UsedImplicitly]
        public MembershipEditViewModel()
        {
            // MVC
        }

        public MembershipEditViewModel(MembershipUser u)
        {
            UserName   = u.UserName;
            Email      = u.Email;
            Comment    = u.Comment;
            IsApproved = u.IsApproved;
        }

        [Required]
        public string UserName
        {
            get;
            set;
        }

        [EmailAddress]
        public string Email
        {
            get;
            set;
        }

        [MembershipPassword]
        public string Password
        {
            get;
            set;
        }

        public string Comment
        {
            get;
            set;
        }

        public bool IsApproved
        {
            get;
            set;
        }

        public IEnumerable<string> Roles
        {
            get;
            set;
        }

        public bool ForEdit
        {
            get;
            set;
        }

        public static List<string> AvailableRoles
        {
            get
            {
                return _availableRoles;
            }
        }
    }
}
