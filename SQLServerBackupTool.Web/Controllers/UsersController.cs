using PagedList;
using SQLServerBackupTool.Lib.Annotations;
using SQLServerBackupTool.Web.Lib.Mvc;
using SQLServerBackupTool.Web.ViewModels;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace SQLServerBackupTool.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : ApplicationController
    {
        private const int NumberItemsPerPage = 30;

        private static MembershipProvider Provider
        {
            get { return Membership.Provider; }
        }

        private static Type MembershipProviderUserKeyType
        {
            get { return typeof(Guid); }
        }

        protected override void Initialize(RequestContext r)
        {
            base.Initialize(r);

            if (!Provider.EnablePasswordReset || Provider.RequiresQuestionAndAnswer)
            {
                throw new ConfigurationErrorsException(
                    "UsersController requires membership configuration options : enablePasswordReset -> true and requiresQuestionAndAnswer -> false"
                );
            }
        }

        /**
         * Index
         */

        public ActionResult Index()
        {
            int pageIndex;

            if (!int.TryParse(Request.Params["page"], out pageIndex))
            {
                pageIndex = 1;
            }

            int totalRecords;

            var users = Provider.GetAllUsers(pageIndex - 1, NumberItemsPerPage, out totalRecords);

            var list = users.Cast<MembershipUser>();

            return View(list.ToPagedList(pageIndex, NumberItemsPerPage));
        }

        /**
         * Create
         */

        public ActionResult Create()
        {
            return View(new MembershipEditViewModel
            {
                IsApproved = true,
                Roles      = new string[] { }
            });
        }

        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Create(MembershipEditViewModel u)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    MembershipCreateStatus mStatus;

                    var userName = u.UserName;

                    var newUser = Membership.CreateUser(
                        userName,
                        u.Password,
                        u.Email,
                        null,
                        null,
                        u.IsApproved,
                        out mStatus
                    );

                    if (mStatus == MembershipCreateStatus.Success && newUser != null)
                    {
                        if (newUser.Comment != u.Comment)
                        {
                            newUser.Comment = u.Comment;
                            Provider.UpdateUser(newUser);
                        }

                        HandleRoles(u);

                        AddFlashMessage(string.Format("User '{0}' successfully created", userName), FlashMessageType.Success);

                        return RedirectToAction("Edit", new { id = newUser.ProviderUserKey });
                    }

                    var status = GetMembershipCreateStatusMessage(mStatus);

                    AddFlashMessage(
                        string.Format("An error occurred during user creation : {0}", status),
                        FlashMessageType.Error
                    );
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("An error occurred", ex);
                    AddFlashMessage("An error occurred during user creation", FlashMessageType.Error);
                }
            }
            else
            {
                DebugModelStateErrors();

                AddFlashMessage("Unable to create user with provided values, please correct errors", FlashMessageType.Warning);
            }

            u.Roles = u.Roles ?? new string[] { };

            return View(u);
        }

        /**
         * Edit
         */

        public ActionResult Edit(string id)
        {
            var uKey = GetRealProviderUserKey(id);

            if (uKey == null)
            {
                return HttpNotFound();
            }

            var u = Membership.GetUser(uKey);

            if (u == null)
            {
                return HttpNotFound(string.Format("User : {0} not found", u.UserName));
            }

            return View(new MembershipEditViewModel(u)
            {
                ForEdit = true,
            });
        }

        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Edit(MembershipEditViewModel u)
        {
            var username = u.UserName;
            var mem = Membership.GetUser(username);

            if (mem == null)
            {
                return HttpNotFound(string.Format("User : {0} not found", username));
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(u.Password) && u.Password == u.PasswordConfirmation)
                {
                    mem.ChangePassword(mem.ResetPassword(), u.Password);
                }

                if (mem.Comment != u.Comment)
                {
                    mem.Comment = u.Comment;
                    Provider.UpdateUser(mem);
                }

                HandleRoles(u);

                AddFlashMessage("User successfully modified", FlashMessageType.Success);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                AddFlashMessage("An error occurred during user modification", FlashMessageType.Error);
                Logger.ErrorException("An error occurred", ex);
            }

            u.ForEdit = true;
            return View(u);
        }

        /**
         * User delete
         */

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            var uKey = GetRealProviderUserKey(id);

            if (uKey == null)
            {
                return HttpNotFound();
            }

            var mem = Membership.GetUser(uKey);

            if (mem == null)
            {
                return HttpNotFound();
            }

            var userName = mem.UserName;

            try
            {
                if (Provider.DeleteUser(userName, true))
                {
                    AddFlashMessage(string.Format("User '{0}' successfully deleted", userName), FlashMessageType.Success);
                }
                else
                {
                    AddFlashMessage(string.Format("Unable to delete user '{0}'", userName), FlashMessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                AddFlashMessage(string.Format("An error occurred while deleting user {0}", userName), FlashMessageType.Error);
                Logger.ErrorException("An error occurred", ex);
            }

            return RedirectToAction("Index");
        }

        /**
         * Random password generator
         */

        public ActionResult GeneratePassword()
        {
            return Content(
                Membership.GeneratePassword(
                    Membership.MinRequiredPasswordLength,
                    Membership.MinRequiredNonAlphanumericCharacters
                ),
                "text/plain"
            );
        }

        /// <summary>
        /// Transforms <see cref="id"/> into the real under-laying <see cref="MembershipUser.ProviderUserKey"/> type
        /// </summary>
        /// <param name="id">Key as string</param>
        /// <returns>A trans-typed object or what was initially given</returns>
        protected static object GetRealProviderUserKey(string id)
        {
            object realProviderUserKey = null;

            if (MembershipProviderUserKeyType == typeof(Guid))
            {
                try
                {
                    realProviderUserKey = Guid.Parse(id);
                }
                // ReSharper disable EmptyGeneralCatchClause : What can we do anyway ?
                catch (Exception) { }
                // ReSharper restore EmptyGeneralCatchClause
            }
            else if (MembershipProviderUserKeyType == typeof(int))
            {
                try
                {
                    realProviderUserKey = int.Parse(id);
                }
                // ReSharper disable EmptyGeneralCatchClause : What can we do anyway ?
                catch (Exception) { }
                // ReSharper restore EmptyGeneralCatchClause
            }
            else
            {
                realProviderUserKey = id;
            }

            return realProviderUserKey;
        }

        /// <summary>
        /// When roles are enabled adds/removes roles
        /// </summary>
        /// <param name="u"><see cref="MembershipEditViewModel"/> to get the roles from</param>
        protected static void HandleRoles([NotNull] MembershipEditViewModel u)
        {
            if (u == null)
            {
                throw new ArgumentNullException("u");
            }

            if (!Roles.Enabled)
            {
                return;
            }

            var rolesForUser = Roles.GetRolesForUser(u.UserName);

            if (rolesForUser != null && rolesForUser.Length > 0)
            {
                Roles.RemoveUserFromRoles(u.UserName, rolesForUser);
            }

            if (u.Roles != null && u.Roles.Any())
            {
                Roles.AddUserToRoles(u.UserName, u.Roles.ToArray());
            }
        }

        /// <summary>
        /// Translates a <see cref="MembershipCreateStatus"/> to a human readable string
        /// </summary>
        /// <param name="status">The status to translate</param>
        /// <returns>A status string</returns>
        /// <exception cref="ArgumentOutOfRangeException">Unknown status type</exception>
        protected static string GetMembershipCreateStatusMessage(MembershipCreateStatus status)
        {
            switch (status)
            {
                case MembershipCreateStatus.Success:
                    return "The user was successfully created.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name was not found in the database.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password is not formatted correctly.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password question is not formatted correctly.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password answer is not formatted correctly.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address is not formatted correctly.";

                case MembershipCreateStatus.DuplicateUserName:
                    return "The user name already exists in the database for the application.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "The e-mail address already exists in the database for the application.";

                case MembershipCreateStatus.UserRejected:
                    return "The user was not created, for a reason defined by the provider.";

                case MembershipCreateStatus.InvalidProviderUserKey:
                    return "The provider user key is of an invalid type or format.";

                case MembershipCreateStatus.DuplicateProviderUserKey:
                    return "The provider user key already exists in the database for the application.";

                case MembershipCreateStatus.ProviderError:
                    return
                        "The provider returned an error that is not described by other MembershipCreateStatus enumeration values.";
            }

            throw new ArgumentOutOfRangeException("status");
        }
    }
}
