using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Firebase.Auth;
using Firebase.Database;
using Google.Cloud.Firestore;
using PittClubManager.Models;
using PittClubManager.Util;

namespace PittClubManager.Controllers
{
    public class ClubController : Controller
    {
        /** 
         * endpont: /club/id/{id}
         * returns a club dashboard for a club of a given id
         **/

        String EMAIL = "jmaciak14@gmail.com";
        String PASSWORD = "password";

        public ActionResult Id(int id)
        {
            // todo: this should eventually fetch the club from the db by id
            Models.User user = FirebaseHelper.GetUser("SAMPLE_USER").Result;

            return View("Id", null);
        }
            }
}
