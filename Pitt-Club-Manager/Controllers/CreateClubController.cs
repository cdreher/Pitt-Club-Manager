using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PittClubManager.Util;

namespace PittClubManager.Controllers
{
    public class CreateClubController : Controller
    {
        class Model
        {
            public Boolean success;
            public Boolean IsSuccess() {
                return success;

            }
        }
        [HttpGet]
        public ActionResult Index(string result)
        {
            Models.User user = (Models.User)Session["CurUser"];
            // check if the user is logged in. If they are logged in, check if they are in the club
            // if they are in the club, they should be able to see everything. Otherwise, there should be a button to request to join
            if (user == null || user.GetId() == "###")
            { 
                return RedirectToAction("Index", "Login");
            }
            Boolean success = "success".Equals(result);
            return View (success);
        }

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            Models.User user = (Models.User)Session["CurUser"];

            if (user != null && user.GetId() != "###")
            {
                FirebaseHelper.CreateClub(collection["name"], collection["description"], user.GetId());
            }
            return RedirectToAction("Index", new { result = "success" });
        }
    }
}