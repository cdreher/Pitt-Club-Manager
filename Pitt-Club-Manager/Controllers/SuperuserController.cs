﻿using System;
using System.Collections;
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

    }
}