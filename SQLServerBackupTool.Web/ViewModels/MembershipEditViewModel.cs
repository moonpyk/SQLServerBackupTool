using SQLServerBackupTool.Lib.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Security;

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
            UserName    = u.UserName;
            Email       = u.Email;
            Comment     = u.Comment;
            IsApproved  = u.IsApproved;
            IsLockedOut = u.IsLockedOut;
            IsOnline    = u.IsOnline;
            InnerUser   = u;
        }

        [Required, Display(Name = "Username")]
        public string UserName
        {
            get;
            set;
        }

        [EmailAddress, Display(Name = "E-mail address")]
        public string Email
        {
            get;
            set;
        }

        [MembershipPassword, Display(Name = "Password")]
        public string Password
        {
            get;
            set;
        }

        [Compare("Password", ErrorMessage = "Password confirmation and password must be the same")]
        public string PasswordConfirmation
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

        public bool IsLockedOut
        {
            get;
            set;
        }

        public bool IsOnline
        {
            get;
            set;
        }

        public bool ForEdit
        {
            get;
            set;
        }

        public IEnumerable<string> Roles
        {
            get;
            set;
        }

        public MembershipUser InnerUser
        {
            get;
            private set;
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
