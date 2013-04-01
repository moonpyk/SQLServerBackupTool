using PagedList;
using SQLServerBackupTool.Web.Lib.Mvc;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using SQLServerBackupTool.Web.ViewModels;

namespace SQLServerBackupTool.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : ApplicationController
    {
        private const int NumberItemsPerPage = 30;

        //
        // GET: /Users/
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
            return View();
        }

        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Create(MembershipUser u)
        {
            return RedirectToAction("Edit", new { id=u.UserName });
        }

        /**
         * Edit
         */

        public ActionResult Edit(string id)
        {
            var u = Membership.GetUser(id);

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
            var mem = Membership.GetUser(u.UserName);

            if (mem == null)
            {
                return HttpNotFound(string.Format("User : {0} not found", u.UserName));
            }

            if (!string.IsNullOrWhiteSpace(u.Password) && u.Password == u.PasswordConfirmation)
            {
                mem.ChangePassword(mem.ResetPassword(), u.Password);
            }

            mem.Comment = u.Comment;
            Provider.UpdateUser(mem);

            AddFlashMessage("User successfuly modified", FlashMessageType.Success);

            return RedirectToAction("Index");
        }

        private static MembershipProvider Provider
        {
            get { return Membership.Provider; }
        }
    }
}
