using System;
using System.Collections;
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
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ArrayList x = new ArrayList();
            x = FirebaseHelper.GetClubList().Result;
            ViewBag.length = x.Count;
            //ViewBag.user = FirebaseHelper.GetUser()
            return View("Index", x);
        }
    }
}
