using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PagedList;
using SQLServerBackupTool.Web.Lib.Mvc;

namespace SQLServerBackupTool.Web.Controllers
{
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

            var list = users.Cast<MembershipUser>().ToList();

            return View(list.ToPagedList(pageIndex, NumberItemsPerPage));
        }

        private static MembershipProvider Provider
        {
            get { return Membership.Provider; }
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
    }
}
