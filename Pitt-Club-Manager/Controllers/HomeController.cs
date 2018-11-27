using System.Web.Mvc;

namespace PittClubManager.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Models.User curUser = (Models.User)Session["CurUser"];
            if(curUser != null)
            {
                System.Diagnostics.Debug.WriteLine("Current user is: " + curUser.GetFirstName());
            } else
            {
                System.Diagnostics.Debug.WriteLine("No user logged in.");
            }
            return View();
        }
    }
}
