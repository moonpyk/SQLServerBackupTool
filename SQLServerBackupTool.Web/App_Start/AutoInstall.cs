using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web.Security;
using Dapper;
using SQLServerBackupTool.Web;
using SQLServerBackupTool.Web.Lib;
using SQLServerBackupTool.Web.Models;

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
            BasicMembershipAuthHttpModule.Realm = "SSBT.web";

            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<SSBTDbContext>());
            using (var ddb = new SSBTDbContext())
            {
                ddb.Database.Initialize(false);
            }

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