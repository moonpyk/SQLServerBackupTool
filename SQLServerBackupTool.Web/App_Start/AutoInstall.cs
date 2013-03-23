using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web.Security;
using Dapper;
using SQLServerBackupTool.Web;

[assembly: WebActivator.PreApplicationStartMethod(typeof(AutoInstall), "PreStart")]
[assembly: WebActivator.PostApplicationStartMethod(typeof(AutoInstall), "PostStart")]

namespace SQLServerBackupTool.Web
{

    public static class AutoInstall
    {
        public static void PreStart()
        {
            Debug.WriteLine("PreStart");
        }

        public static void PostStart()
        {
            MembershipUser defaultUser = null;
            if (Membership.GetAllUsers().Count == 0)
            {
                defaultUser = Membership.CreateUser("admin", "password");
            }

            if (!Roles.RoleExists("Admin"))
            {
                Roles.CreateRole("Admin");

                if (defaultUser != null)
                {
                    Roles.AddUserToRole(defaultUser.UserName, "Admin");
                }
            }
        }
    }
}