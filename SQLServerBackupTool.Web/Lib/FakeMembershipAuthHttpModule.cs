using SQLServerBackupTool.Lib.Annotations;
using System;
using System.Security.Principal;
using System.Threading;
using System.Web;

namespace SQLServerBackupTool.Web.Lib
{
    [UsedImplicitly]
    public class FakeMembershipAuthHttpModule : IHttpModule
    {
        private static readonly GenericPrincipal FakePrincipal = new GenericPrincipal(new GenericIdentity("admin"), new[] { "Admin" });

        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += OnApplicationAuthenticateRequest;
        }

        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;

            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }

        private static void OnApplicationAuthenticateRequest(object sender, EventArgs e)
        {
            SetPrincipal(FakePrincipal);
        }

        public void Dispose()
        {

        }
    }
}
