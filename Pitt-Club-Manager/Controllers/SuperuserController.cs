using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PittClubManager.Util;
namespace PittClubManager.Controllers
{
    public class SuperuserController : Controller
    {
        // GET: Superuser
        public ActionResult Index()
        {
            var pendingClubs = FirebaseHelper.GetPendingClubs();
            return View(pendingClubs);
        }

        public ActionResult ApproveClub(string id)
        {
            FirebaseHelper.ApproveClub(id);
            return View();
        }

        public ActionResult CreateClub(FormCollection formCollection) {
            // todo: will be able to get user id from session
            FirebaseHelper.CreateClub(formCollection["name"], formCollection["description"], "todo: implement");
            return RedirectToAction("Index", "Superuser");
        }

    }
}