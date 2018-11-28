using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Firebase.Auth;
using Firebase.Database;
using Google.Cloud.Firestore;
using Grpc.Core;
using Grpc.Core.Logging;
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

        private void DummyWebRequest() {
            string html = string.Empty;
            string url = @"https://api.stackexchange.com/2.2/answers?order=desc&sort=activity&site=stackoverflow";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            Console.WriteLine(html);
        }

        public ActionResult Join()
        {
            //System.Diagnostics.Debug.WriteLine("Attempting to join club and return to " + (string)Session["ClubId"]);


            Models.User curUser = (Models.User)Session["CurUser"];
            var b = FirebaseHelper.RequestJoinClub(curUser.GetId(), (string)Session["ClubId"]).Result;

            return RedirectToAction("Id", "Club", new {id = (string)Session["ClubId"] });
            //var c = FirebaseHelper.GetClub("SAMPLE_CLUB").Result;
            //return View("SAMPLE_CLUB", c);
        }

        public ActionResult Id(String id)
        {
            // todo: this should eventually fetch the club from the db by id
            var c = FirebaseHelper.GetClub(id).Result;
            /*Task<System.Collections.ArrayList> clTask = FirebaseHelper.GetClubList();
            clTask.Wait();
            //System.Collections.ArrayList cl = clTask.Result;
            //System.Diagnostics.Debug.WriteLine(cl.Count);
            for (int i = 0; i < cl.Count; i++)
            {
                Club cur = (PittClubManager.Models.Club)cl[i];
                System.Diagnostics.Debug.WriteLine(cur.GetId());
            }*/
            Session["ClubId"] = id;
            System.Diagnostics.Debug.WriteLine("Set clubId to " + (string)Session["ClubId"]);
            TempData["CurUserInClub"] = false;
            TempData["UserLoggedIn"] = false;
            TempData["CurUserPendingApproval"] = false;
            Models.User curUser = (Models.User)Session["CurUser"];
            // check if the user is logged in. If they are logged in, check if they are in the club
            // if they are in the club, they should be able to see everything. Otherwise, there should be a button to request to join
            if(curUser != null && curUser.GetId() != "###")
            {
                TempData["UserLoggedIn"] = true;
                TempData["CurUserInClub"] = FirebaseHelper.UserInClub(curUser.GetId(), id);
                TempData["CurUserPendingApproval"] = FirebaseHelper.UserPendingClubApproval(curUser.GetId(), id);
            }
            return View("Id", c);
        }
    }
}
