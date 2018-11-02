using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PittClubManager.Models;

namespace PittClubManager.Controllers
{
    public class ClubController : Controller
    {
        /** 
         * endpont: /club/id/{id}
         * returns a club dashboard for a club of a given id
         **/
        public ActionResult Id(int id)
        {
            // todo: this should eventually fetch the club from the db by id
            Club club = GetClub(id);
            return View ("Id", club);
        }

        private Club GetClub(int id) {
            Club mock = new Club();
            mock.SetId(id);
            mock.SetName("Pitt Lawn Sports");
            mock.SetManager(new User(1, UserPermission.MANAGER, "Josh", "Maciak"));
            mock.SetMembers(new User[] { new User(2, UserPermission.MANAGER, "Member", "McMemberface") });
            mock.SetEvents(new Event[] { new Event(1, DateTime.Now, "Party", "A super cool party") });
            return mock;
        }
    }
}
