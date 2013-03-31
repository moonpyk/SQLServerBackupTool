using PagedList;
using SQLServerBackupTool.Web.Lib.Mvc;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

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

        public ActionResult Create()
        {
            return View();
        }

        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Create(MembershipUser u)
        {
            return RedirectToAction("Edit", new { id=u.UserName });
        }

        public ActionResult Edit(string id)
        {
            var u = Membership.GetUser(id);

            if (u == null)
            {
                return HttpNotFound();
            }

            return View(u);
        }

        [ValidateAntiForgeryToken, HttpPost]
        public ActionResult Edit(MembershipUser u)
        {
            return RedirectToAction("Index");
        }

        private static MembershipProvider Provider
        {
            get { return Membership.Provider; }
        }
    }
}
