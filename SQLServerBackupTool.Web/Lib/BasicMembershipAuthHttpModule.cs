using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;

namespace SQLServerBackupTool.Web.Lib
{
    public class BasicMembershipAuthHttpModule : IHttpModule
    {
        private static string _realm = "Access restricted area";

        public static string Realm
        {
            get { return _realm; }
            set { _realm = value; }
        }

        public void Init(HttpApplication context)
        {
            // Register event handlers
            context.AuthenticateRequest += OnApplicationAuthenticateRequest;
            context.EndRequest += OnApplicationEndRequest;
        }

        public void Dispose()
        {
        }

        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;

            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }

        private static bool AuthenticateUser(string credentials)
        {
            var validated = false;

            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                credentials = encoding.GetString(Convert.FromBase64String(credentials));

                var separator = credentials.IndexOf(':');
                var name = credentials.Substring(0, separator);
                var password = credentials.Substring(separator + 1);

                try
                {
                    validated = Membership.ValidateUser(name, password);
                    if (validated)
                    {
                        var user = Membership.GetUser(name, true);

                        if (user != null)
                        {
                            var identity = new GenericIdentity(user.UserName);

                            string[] rolesForUser = null;

                            if (Roles.Enabled)
                            {
                                rolesForUser = Roles.GetRolesForUser(user.UserName);
                            }

                            SetPrincipal(new GenericPrincipal(identity, rolesForUser));
                        }
                    }
                }
                catch (EntityCommandExecutionException, DbUpdateException)
                {
                    // Weird Universal providers exception see : http://connect.microsoft.com/VisualStudio/feedback/details/751178/membership-getuser-and-membership-validateuser-deadlock
                }
            }
            catch (FormatException)
            {
                // Credentials were not formatted correctly.
                validated = false;
            }

            return validated;
        }

        private static void OnApplicationAuthenticateRequest(object sender, EventArgs e)
        {
            var request = HttpContext.Current.Request;
            var authHeader = request.Headers["Authorization"];
            if (authHeader == null)
            {
                return;
            }

            var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);

            // RFC 2617 sec 1.2, "scheme" name is case-insensitive
            if (
                authHeaderVal.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) &&
                authHeaderVal.Parameter != null
                )
            {
                AuthenticateUser(authHeaderVal.Parameter);
            }
        }

        // If the request was unauthorized, add the WWW-Authenticate header 
        // to the response.
        private static void OnApplicationEndRequest(object sender, EventArgs e)
        {
            var response = HttpContext.Current.Response;
            if (response.StatusCode == 401)
            {
                response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", Realm));
            }
        }
    }
}