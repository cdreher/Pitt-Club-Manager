using System;
using System.Web.Mvc;
using Firebase.Auth;
using Firebase.Database;
using PittClubManager.Models;

namespace PittClubManager.Controllers
{
    public class LoginController : Controller
    {
        string _email { get; set; }
        string _password { get; set; }
        Models.User user = new Models.User("###", "John", "Doe");
        public static FirebaseClient firebase { get; set; }
        public static FirebaseAuthProvider authProvider { get; set; }

        public ActionResult Index()
        {
            if (TempData["InvalidLogin"] == null)
            {
                TempData["InvalidLogin"] = false;
            }
            return View ();
        }

        [HttpPost]
        public ActionResult Login(FormCollection formCollection)
        {
            _email = formCollection["login-email"];
            _password = formCollection["login-password"];

            firebase = new FirebaseClient("https://pitt-club-manager.firebaseio.com");
            authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCN8Av2-nfNtsRdlWaZiaejPdwQ4QqA38c"));

            authProvider.SignInWithEmailAndPasswordAsync(_email, _password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Console.WriteLine("SignInWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Console.WriteLine("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                FirebaseAuth newUser = task.Result;
                Console.WriteLine("SignInWithEmailAndPasswordAsync success: {0}, {1}", _email, _password);

                user.SetId(newUser.User.LocalId);


            });
            System.Threading.Thread.Sleep(500);

            if (user.GetId().Equals("###"))
            {
                TempData["InvalidLogin"] = true;
                return RedirectToAction("Index", "Login");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Register()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
